using System.Collections.Generic;
using UnityEngine;

public class FlowFieldManager : MonoBehaviour
{
    private static FlowFieldManager instance;
    public static FlowFieldManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<FlowFieldManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FlowFieldManager");
                    instance = go.AddComponent<FlowFieldManager>();
                }
            }
            return instance;
        }
    }

    private const byte COST_WALKABLE = 1;
    private const byte COST_IMPASSABLE = byte.MaxValue;
    private const int ORTHOGONAL_COST = 10;
    private const int DIAGONAL_COST = 14;

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 44;
    [SerializeField] private int gridHeight = 44;
    [SerializeField] private float cellSize = 0.75f;
    [Tooltip("Hệ số nhân kích thước hộp quét vật cản (> 1 giúp quái không bị quệt rìa collider vào góc đá)")]
    [SerializeField] private float obstacleBoxScale = 1.15f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Update Settings")]
    [SerializeField] private float periodicRebuildInterval = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private Transform player;
    private byte[,] costField;
    private int[,] integrationField;
    private Vector2[,] flowField;
    private Vector2 gridOriginWorld;
    private Vector2Int lastPlayerWorldCell = new Vector2Int(int.MinValue, int.MinValue);
    private float rebuildTimer;

    private readonly Queue<Vector2Int> openQueue = new Queue<Vector2Int>(1024);

    private static readonly Vector2Int[] Directions8 =
    {
        new Vector2Int( 0,  1), // Up
        new Vector2Int( 0, -1), // Down
        new Vector2Int(-1,  0), // Left
        new Vector2Int( 1,  0), // Right
        new Vector2Int(-1,  1), // Up-Left
        new Vector2Int( 1,  1), // Up-Right
        new Vector2Int(-1, -1), // Down-Left
        new Vector2Int( 1, -1)  // Down-Right
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (obstacleLayer.value == 0)
        {
            obstacleLayer = LayerMask.GetMask("Obstacle", "Wall");
        }

        AllocateBuffers();
    }

    private void Start()
    {
        FindPlayerIfNeeded();
        if (player != null)
        {
            RebuildFlowField(WorldToWorldCell(player.position));
        }
    }

    private void Update()
    {
        FindPlayerIfNeeded();
        if (player == null) return;

        rebuildTimer += Time.deltaTime;
        Vector2Int currentPlayerCell = WorldToWorldCell(player.position);

        if (currentPlayerCell != lastPlayerWorldCell || rebuildTimer >= periodicRebuildInterval)
        {
            rebuildTimer = 0f;
            lastPlayerWorldCell = currentPlayerCell;
            RebuildFlowField(currentPlayerCell);
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(CONSTANT.PLAYER_TAG);
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void AllocateBuffers()
    {
        costField = new byte[gridWidth, gridHeight];
        integrationField = new int[gridWidth, gridHeight];
        flowField = new Vector2[gridWidth, gridHeight];
    }

    private Vector2Int WorldToWorldCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize)
        );
    }

    private Vector2 CellToWorldCenter(int gx, int gy)
    {
        return new Vector2(
            gridOriginWorld.x + (gx + 0.5f) * cellSize,
            gridOriginWorld.y + (gy + 0.5f) * cellSize
        );
    }

    private bool WorldToLocalGrid(Vector2 worldPos, out int gx, out int gy)
    {
        gx = Mathf.FloorToInt((worldPos.x - gridOriginWorld.x) / cellSize);
        gy = Mathf.FloorToInt((worldPos.y - gridOriginWorld.y) / cellSize);
        return InBounds(gx, gy);
    }

    private bool InBounds(int gx, int gy)
    {
        return gx >= 0 && gx < gridWidth && gy >= 0 && gy < gridHeight;
    }

    private void RebuildFlowField(Vector2Int centerWorldCell)
    {
        if (costField == null || costField.GetLength(0) != gridWidth || costField.GetLength(1) != gridHeight)
        {
            AllocateBuffers();
        }

        int halfW = gridWidth / 2;
        int halfH = gridHeight / 2;

        gridOriginWorld = new Vector2(
            (centerWorldCell.x - halfW) * cellSize,
            (centerWorldCell.y - halfH) * cellSize
        );

        Vector2Int targetCell = new Vector2Int(halfW, halfH);
        Vector2 checkSize = Vector2.one * (cellSize * obstacleBoxScale);

        // 1. Tạo Cost Field & Reset Integration Field
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 cellCenter = CellToWorldCenter(x, y);
                bool hasObstacle = Physics2D.OverlapBox(cellCenter, checkSize, 0f, obstacleLayer) != null;

                costField[x, y] = hasObstacle ? COST_IMPASSABLE : COST_WALKABLE;
                integrationField[x, y] = int.MaxValue;
                flowField[x, y] = Vector2.zero;
            }
        }

        // Luôn đảm bảo ô của Player có thể đi tới được để luồng BFS tỏa ra xung quanh
        costField[targetCell.x, targetCell.y] = COST_WALKABLE;
        integrationField[targetCell.x, targetCell.y] = 0;

        // 2. Tạo Integration Field (BFS / Wavefront)
        openQueue.Clear();
        openQueue.Enqueue(targetCell);

        while (openQueue.Count > 0)
        {
            Vector2Int current = openQueue.Dequeue();
            int currentCost = integrationField[current.x, current.y];

            for (int i = 0; i < Directions8.Length; i++)
            {
                Vector2Int dir = Directions8[i];
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (!InBounds(nx, ny)) continue;
                if (costField[nx, ny] == COST_IMPASSABLE) continue;

                // Chống cắt góc chéo xuyên qua khe hẹp của vật cản
                bool isDiagonal = dir.x != 0 && dir.y != 0;
                if (isDiagonal)
                {
                    if (costField[current.x + dir.x, current.y] == COST_IMPASSABLE ||
                        costField[current.x, current.y + dir.y] == COST_IMPASSABLE)
                    {
                        continue;
                    }
                }

                int stepCost = isDiagonal ? DIAGONAL_COST : ORTHOGONAL_COST;
                int newCost = currentCost + stepCost * costField[nx, ny];

                if (newCost < integrationField[nx, ny])
                {
                    integrationField[nx, ny] = newCost;
                    openQueue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        // 3. Tạo Flow Field (Vector hướng đi cho từng ô)
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int bestCost = integrationField[x, y];
                Vector2 bestDir = Vector2.zero;

                for (int i = 0; i < Directions8.Length; i++)
                {
                    Vector2Int dir = Directions8[i];
                    int nx = x + dir.x;
                    int ny = y + dir.y;

                    if (!InBounds(nx, ny)) continue;

                    bool isDiagonal = dir.x != 0 && dir.y != 0;
                    if (isDiagonal && costField[x, y] != COST_IMPASSABLE)
                    {
                        if (costField[x + dir.x, y] == COST_IMPASSABLE ||
                            costField[x, y + dir.y] == COST_IMPASSABLE)
                        {
                            continue;
                        }
                    }

                    int neighborCost = integrationField[nx, ny];
                    if (neighborCost < bestCost)
                    {
                        bestCost = neighborCost;
                        bestDir = new Vector2(dir.x, dir.y).normalized;
                    }
                }

                flowField[x, y] = bestDir;
            }
        }
    }

    public Vector2 GetFlowDirection(Vector2 worldPos, Transform fallbackTarget = null)
    {
        Transform tgt = fallbackTarget != null ? fallbackTarget : player;
        if (tgt == null) return Vector2.zero;

        Vector2 toTarget = (Vector2)tgt.position - worldPos;

        // Khi đã ở rất gần Player (< 1.2 ô), đi thẳng vào tâm Player để vây quanh mượt mà
        if (toTarget.sqrMagnitude <= (cellSize * 1.2f) * (cellSize * 1.2f))
        {
            return toTarget.normalized;
        }

        if (flowField != null && WorldToLocalGrid(worldPos, out int gx, out int gy))
        {
            Vector2 flowDir = flowField[gx, gy];
            if (flowDir.sqrMagnitude > 0.001f)
            {
                return flowDir;
            }
        }

        return toTarget.normalized;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos || flowField == null || costField == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 center = CellToWorldCenter(x, y);

                if (costField[x, y] == COST_IMPASSABLE)
                {
                    Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
                    Gizmos.DrawCube(center, Vector3.one * (cellSize * 0.9f));
                    continue;
                }

                Vector2 dir = flowField[x, y];
                if (dir.sqrMagnitude > 0.01f)
                {
                    Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.6f);
                    Gizmos.DrawLine(center, center + dir * (cellSize * 0.4f));
                }
            }
        }
    }
#endif
}

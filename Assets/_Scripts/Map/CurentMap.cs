using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CurrentMap : MonoBehaviour
{           
    MapController mc;
    public GameObject targetMap;
    [SerializeField] private MapData mapData;
    [SerializeField] private List<Transform> spawnObstaclesPoints = new List<Transform>();
    [SerializeField] private List<GameObject> spawnedObstacles = new List<GameObject>();

    private void Awake()
    {
        mc = Object.FindFirstObjectByType<MapController>();
        targetMap = gameObject; // <= tự động set chính instance đang chứa collider

        // Prefer mapData values when available, otherwise try to load PosX children from targetMap
        if (mapData != null && mapData.spawnobstaclesPoints != null && mapData.spawnobstaclesPoints.Count > 0)
            spawnObstaclesPoints = mapData.spawnobstaclesPoints;
        else
            LoadPosPointsFromChildren(); // loads children named "Pos1", "Pos2", ...

        spawnedObstacles = new List<GameObject>(mapData != null && mapData.obstacles != null ? mapData.obstacles : spawnedObstacles);
    }

    void Start()
    {
        // By default use unique points (no two obstacles share the same spawn point).
        // If you want to allow multiple obstacles at the same point, call RandomizeObstacles(true).
        RandomizeObstacles(allowMultiplePerPoint: false);
    }

    /// <summary>
    /// Find child Transforms under the targetMap (or this GameObject) whose names match "Pos{number}" (e.g. Pos1, Pos2).
    /// Matches are sorted by their numeric suffix and assigned to <see cref="spawnObstaclesPoints"/>.
    /// Use the optional parent parameter to specify a different container Transform.
    /// </summary>
    /// <param name="prefix">The name prefix, default "Pos".</param>
    /// <param name="parent">Optional parent to search. If null the method searches under targetMap or this.transform.</param>
    [ContextMenu("LoadPosPointsFromChildren")]
    public void LoadPosPointsFromChildren(string prefix = "Pos", Transform parent = null)
    {
        Transform searchRoot = parent != null ? parent : (targetMap != null ? targetMap.transform : this.transform);
        var matches = new List<(Transform t, int idx)>();

        foreach (Transform child in searchRoot)
        {
            var m = Regex.Match(child.name, $"^{Regex.Escape(prefix)}(\\d+)$");
            if (m.Success)
            {
                if (int.TryParse(m.Groups[1].Value, out int idx))
                {
                    matches.Add((child, idx));
                }
            }
        }

        // Sort by the numeric suffix (ascending) and store only the Transforms
        spawnObstaclesPoints = matches
            .OrderBy(x => x.idx)
            .Select(x => x.t)
            .ToList();

        if (spawnObstaclesPoints.Count == 0)
            Debug.LogWarning($"[CurrentMap] No child points found with pattern '{prefix}{{number}}' under '{searchRoot.name}'.");
    }

    /// <summary>
    /// Randomly instantiate the obstacle prefabs in <see cref="spawnedObstacles"/> onto positions from <see cref="spawnObstaclesPoints"/>.
    /// If allowMultiplePerPoint == false, each used spawn point will be unique (max min(obstacles, points) spawned).
    /// If allowMultiplePerPoint == true, each obstacle chooses a random point and multiple obstacles may share a point.
    /// </summary>
    /// <param name="allowMultiplePerPoint">Allow multiple obstacles to use the same spawn point.</param>
    [ContextMenu("RandomizeObstacles")]
    public void RandomizeObstacles(bool allowMultiplePerPoint = false)
    {
        if (mapData == null && (spawnedObstacles == null || spawnedObstacles.Count == 0))
        {
            Debug.LogWarning("[CurrentMap] mapData and spawnedObstacles are null/empty. Cannot spawn obstacles.");
            return;
        }

        var points = spawnObstaclesPoints != null && spawnObstaclesPoints.Count > 0
            ? spawnObstaclesPoints
            : (mapData != null ? mapData.spawnobstaclesPoints : null);

        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("[CurrentMap] No spawn points available (spawnObstaclesPoints / mapData.spawnobstaclesPoints).");
            return;
        }

        if (spawnedObstacles == null || spawnedObstacles.Count == 0)
        {
            Debug.LogWarning("[CurrentMap] No obstacles defined to spawn (spawnedObstacles).");
            return;
        }

        // Find or create a container under the map to keep spawned obstacles organized
        Transform container = (targetMap != null ? targetMap.transform : this.transform).Find("Obstacles");
        if (container == null)
        {
            GameObject containerGO = new GameObject("Obstacles");
            container = containerGO.transform;
            container.SetParent(targetMap != null ? targetMap.transform : this.transform);
            container.localPosition = Vector3.zero;
        }
        else
        {
            // Clear previously spawned obstacle instances
#if UNITY_EDITOR
            // In editor use DestroyImmediate to immediately remove child objects
            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);
#else
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
#endif
        }

        // Prepare indices for unique-point placement
        List<int> pointIndices = Enumerable.Range(0, points.Count).ToList();
        // Shuffle indices (Fisher-Yates)
        for (int i = pointIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = pointIndices[i];
            pointIndices[i] = pointIndices[j];
            pointIndices[j] = tmp;
        }

        if (allowMultiplePerPoint)
        {
            // Each obstacle picks a random point (points may repeat)
            foreach (var prefab in spawnedObstacles)
            {
                if (prefab == null) continue;
                int idx = Random.Range(0, points.Count);
                Instantiate(prefab, points[idx].position, Quaternion.identity, container);
            }
        }
        else
        {
            // One obstacle per unique point up to the smaller count
            int count = Mathf.Min(spawnedObstacles.Count, points.Count);
            for (int i = 0; i < count; i++)
            {
                var prefab = spawnedObstacles[i];
                if (prefab == null) continue;
                int pointIdx = pointIndices[i];
                Instantiate(prefab, points[pointIdx].position, Quaternion.identity, container);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            mc.currentMap = targetMap;
        }
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            mc.currentMap = targetMap;
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            mc.currentMap = null;
        }
    }
}

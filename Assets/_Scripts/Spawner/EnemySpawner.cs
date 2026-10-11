using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WaveSegment
{
    public GameObject prefab;
    public int enemyCount;
    public float spawnDelay;
}

[System.Serializable]
public struct Wave
{
    public string waveName;
    public WaveSegment[] segments;
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Reference")]
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField, Range(0f, 360f)] private float arcAngle = 120f;
    [SerializeField] private bool spawnInFrontOfPlayer = true;

    [Header("Time-Driven Wave & Quota Settings")]
    [Tooltip("Thời gian tối đa của mỗi Wave thường trước khi tự động sang Wave tiếp theo")]
    [SerializeField] private float waveDuration = 60f;
    [Tooltip("Số lượng quái tối thiểu luôn duy trì trên màn hình trong Wave thường")]
    [SerializeField] private int minEnemiesOnScreen = 10;
    [Tooltip("Tốc độ spawn bù nhanh khi số quái trên sân thấp hơn minEnemiesOnScreen")]
    [SerializeField] private float quotaRefillInterval = 0.25f;

    [Header("Teleport Wrapping (Đón đầu khi quái bị bỏ quá xa)")]
    [SerializeField] private float maxEnemyDistance = 18f;
    [SerializeField] private float wrapCheckInterval = 0.5f;

    [Header("Waves")]
    [SerializeField] private Wave[] waves;
    public int CurrentWaveIndex => currentWaveIndex;
    private int currentWaveIndex;
    private int currentSegmentIndex;

    private int spawnedInSegment;
    private int aliveEnemyCount;

    private float spawnTimer;
    private float currentWaveTimer;
    private float wrapTimer;
    private PlayerInput playerInput;
    private LayerMask obstacleLayer;

    private readonly List<Slime_Base> activeEnemies = new();

    [Header("Loop difficulty scaling (applied when all waves complete and we wrap)")]
    [Tooltip("When waves loop, this int multiplier is applied to spawned enemies' health. Default 2 => double.")]
    [SerializeField] private int healthLoopMultiplier = 2;
    [Tooltip("When waves loop, this int multiplier is applied to spawned enemies' damage. Default 2 => double.")]
    [SerializeField] private int damageLoopMultiplier = 2;

    // Current runtime integer multipliers applied to spawned enemies (1 = normal)
    private int currentHealthMultiplier = 1;
    private int currentDamageMultiplier = 1;

    private void Awake()
    {
        Instance = this;
        obstacleLayer = LayerMask.GetMask("Obstacle", "Wall");
    }

    private void Start()
    {
        if (player != null)
            playerInput = player.GetComponent<PlayerInput>();
        UpdateBGM();
    }

    private void Update()
    {
        if (waves == null || waves.Length == 0) return;

        Wave wave = waves[currentWaveIndex];
        if (wave.segments == null || wave.segments.Length == 0) return;

        WaveSegment segment = wave.segments[currentSegmentIndex];
        bool isBossWave = IsBossWave();

        spawnTimer += Time.deltaTime;
        currentWaveTimer += Time.deltaTime;
        wrapTimer += Time.deltaTime;

        // ===== 1. SPAWN THEO SEGMENT HOẶC BÙ ĐỊNH MỨC TỐI THIỂU (QUOTA) =====
        bool needsQuotaRefill = !isBossWave && aliveEnemyCount < minEnemiesOnScreen;
        float effectiveDelay = needsQuotaRefill
            ? Mathf.Min(segment.spawnDelay, quotaRefillInterval)
            : segment.spawnDelay;

        if (spawnedInSegment < segment.enemyCount && spawnTimer >= effectiveDelay)
        {
            spawnTimer = 0f;
            SpawnEnemy(segment.prefab);
            spawnedInSegment++;
        }
        else if (needsQuotaRefill && AllSegmentsSpawned() && spawnTimer >= quotaRefillInterval)
        {
            // Nếu người chơi dọn sạch quái quá nhanh trước khi hết giờ Wave, tiếp tục bơm thêm quái để không bị trống màn hình
            spawnTimer = 0f;
            SpawnEnemy(segment.prefab);
        }

        // ===== 2. SANG SEGMENT TIẾP THEO =====
        if (spawnedInSegment >= segment.enemyCount)
        {
            MoveToNextSegmentIfNeeded();
        }

        // ===== 3. KẾT THÚC WAVE (THEO THỜI GIAN HOẶC DỌN SẠCH) =====
        bool waveTimeExpired = !isBossWave && currentWaveTimer >= waveDuration;
        bool allCleared = AllSegmentsSpawned() && aliveEnemyCount == 0;

        if (waveTimeExpired || allCleared)
        {
            NextWave();
        }

        // ===== 4. TELEPORT WRAPPING (ĐÓN ĐẦU KHI QUÁI BỊ BỎ LẠI QUÁ XA) =====
        if (wrapTimer >= wrapCheckInterval)
        {
            wrapTimer = 0f;
            RepositionFarEnemies();
        }
    }

    private bool IsBossWave()
    {
        return waves != null && waves.Length > 1 && currentWaveIndex == waves.Length - 1;
    }

    private void UpdateBGM()
    {
        if (waves == null || waves.Length == 0) return;
        bool isBossWave = IsBossWave();
        EventBus<OnWaveChangedEvent>.Publish(new OnWaveChangedEvent(currentWaveIndex, isBossWave));
    }

    // =========================
    // SEGMENT / WAVE LOGIC
    // =========================

    private void MoveToNextSegmentIfNeeded()
    {
        if (currentSegmentIndex < waves[currentWaveIndex].segments.Length - 1)
        {
            currentSegmentIndex++;
            spawnedInSegment = 0;
            spawnTimer = 0f;
        }
    }

    private bool AllSegmentsSpawned()
    {
        return currentSegmentIndex >= waves[currentWaveIndex].segments.Length - 1 &&
               spawnedInSegment >= waves[currentWaveIndex].segments[currentSegmentIndex].enemyCount;
    }

    private void NextWave()
    {
        currentWaveIndex++;

        // If we've passed the last wave, wrap and apply scaling
        if (currentWaveIndex >= waves.Length)
        {
            currentWaveIndex = 0;

            currentHealthMultiplier *= Mathf.Max(1, healthLoopMultiplier);
            currentDamageMultiplier *= Mathf.Max(1, damageLoopMultiplier);

            Debug.Log($"Waves looped. New health multiplier: {currentHealthMultiplier}, damage multiplier: {currentDamageMultiplier}");
        }

        currentSegmentIndex = 0;
        spawnedInSegment = 0;
        spawnTimer = 0f;
        currentWaveTimer = 0f;
        UpdateBGM();
    }

    // =========================
    // REGISTER / UNREGISTER & TELEPORT WRAPPING
    // =========================

    public void RegisterEnemy(Slime_Base enemy = null)
    {
        aliveEnemyCount++;
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Slime_Base enemy = null)
    {
        aliveEnemyCount--;
        if (aliveEnemyCount < 0)
            aliveEnemyCount = 0;

        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
        }
    }

    private void RepositionFarEnemies()
    {
        if (player == null) return;

        float maxDistSqr = maxEnemyDistance * maxEnemyDistance;
        Vector2 playerPos = player.position;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Slime_Base enemy = activeEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            // Không dịch chuyển Boss
            if (enemy is Boss) continue;

            float distSqr = ((Vector2)enemy.transform.position - playerPos).sqrMagnitude;
            if (distSqr > maxDistSqr)
            {
                enemy.transform.position = GetValidSpawnPosition();
            }
        }
    }

    // =========================
    // SPAWN
    // =========================

    private void SpawnEnemy(GameObject prefab)
    {
        if (player == null || prefab == null) return;

        Vector3 pos = GetValidSpawnPosition();

        GameObject go = PoolManager.Instance.Spawn(prefab.name, pos);
        if (go == null) return;

        var slime = go.GetComponent<Slime_Base>();
        if (slime != null)
        {
            if (currentHealthMultiplier > 1 || currentDamageMultiplier > 1)
            {
                slime.UpgradeDataSlime(currentHealthMultiplier, currentDamageMultiplier);
            }
        }

        RegisterEnemy(slime);
    }

    private Vector3 GetValidSpawnPosition()
    {
        // Thử tối đa 5 hướng trên cung tròn để tránh spawn đè lên vật cản (đá/nấm)
        for (int attempt = 0; attempt < 5; attempt++)
        {
            Vector2 dir = GetArcDirection();
            Vector3 candidate = player.position + (Vector3)dir * spawnRadius;
            if (Physics2D.OverlapCircle(candidate, 0.45f, obstacleLayer) == null)
            {
                return candidate;
            }
        }

        Vector2 fallbackDir = GetArcDirection();
        return player.position + (Vector3)fallbackDir * spawnRadius;
    }

    private Vector2 GetArcDirection()
    {
        float halfArc = arcAngle * 0.5f;
        float randomAngle = Random.Range(-halfArc, halfArc);

        float baseAngle = Random.Range(0f, 360f);

        if (spawnInFrontOfPlayer && playerInput != null)
        {
            Vector2 moveDir = playerInput.PlayerDirection;
            if (moveDir.sqrMagnitude > 0.01f)
                baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        }

        float rad = (baseAngle + randomAngle) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}

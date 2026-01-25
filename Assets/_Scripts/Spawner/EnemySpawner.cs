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

    [Header("Waves")]
    [SerializeField] private Wave[] waves;
    public int CurrentWaveIndex => currentWaveIndex;
    private int currentWaveIndex;
    private int currentSegmentIndex;

    private int spawnedInSegment;
    private int aliveEnemyCount;

    private float spawnTimer;
    private float EventTimer = 0f;
    private float EventTimerTime = 0f;
    private bool isEventActive = false;
    private PlayerController playerController;

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
    }

    private void Start()
    {
        if (player != null)
            playerController = player.GetComponent<PlayerController>();
        UpdateBGM();
    }

    private void Update()
    {
        if (waves == null || waves.Length == 0) return;

        Wave wave = waves[currentWaveIndex];
        WaveSegment segment = wave.segments[currentSegmentIndex];

        spawnTimer += Time.deltaTime;
        EventTimer += Time.deltaTime;
        // ===== SPAWN THEO SEGMENT =====
        if (spawnedInSegment < segment.enemyCount &&
            spawnTimer >= segment.spawnDelay)
        {
            spawnTimer = 0f;
            SpawnEnemy(segment.prefab);
            spawnedInSegment++;
        }

        // ===== SANG SEGMENT TIẾP =====
        if (spawnedInSegment >= segment.enemyCount)
        {
            MoveToNextSegmentIfNeeded();
        }

        // ===== KẾT THÚC WAVE =====
        if (AllSegmentsSpawned() && aliveEnemyCount == 0)
        {
            NextWave();
        }
        if(EventTimer >= EventManager.instance.eventInterval)
        {
            EventTimer = 0f;
            EventManager.instance.PickRandomEvent();
            isEventActive = true;
        }
        if(isEventActive)
        {
            EventTimerTime += Time.deltaTime;
            if(EventTimerTime >= EventManager.instance.eventTimer)
            {
                EventTimerTime = 0f;
                isEventActive = false;
                EventManager.instance.EndEvent();
            }
        }

    }
    private void UpdateBGM()
    {
        if (currentWaveIndex == waves.Length - 1)
        {
            AudioManager.Instance.PlayBossBGM();
        }
        else
        {
            AudioManager.Instance.PlayNormalBGM();
        }
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

            // apply integer loop multipliers (double health/damage when multiplier == 2)
            currentHealthMultiplier *= Mathf.Max(1, healthLoopMultiplier);
            currentDamageMultiplier *= Mathf.Max(1, damageLoopMultiplier);

            Debug.Log($"Waves looped. New health multiplier: {currentHealthMultiplier}, damage multiplier: {currentDamageMultiplier}");
        }

        currentSegmentIndex = 0;
        spawnedInSegment = 0;
        aliveEnemyCount = 0;
        spawnTimer = 0f;
        UpdateBGM();
    }

    // =========================
    // REGISTER / UNREGISTER
    // =========================

    public void RegisterEnemy()
    {
        aliveEnemyCount++;
    }

    public void UnregisterEnemy()
    {
        aliveEnemyCount--;
        if (aliveEnemyCount < 0)
            aliveEnemyCount = 0;
    }

    // =========================
    // SPAWN
    // =========================

    private void SpawnEnemy(GameObject prefab)
    {
        if (player == null || prefab == null) return;

        Vector2 dir = GetArcDirection();
        Vector3 pos = player.position + (Vector3)dir * spawnRadius;

        GameObject go = PoolManager.Instance.Spawn(prefab.name, pos);
        if (go == null) return;

        // Use Slime.UpgradeDataSlime to apply integer multipliers when spawning.
        // This ensures the spawned Slime's MaxHealth and Damage are multiplied accordingly.
        var slime = go.GetComponent<Slime>();
        if (slime != null)
        {
            if (currentHealthMultiplier > 1 || currentDamageMultiplier > 1)
            {
                slime.UpgradeDataSlime(currentHealthMultiplier, currentDamageMultiplier);
            }
        }

        RegisterEnemy();
    }

    private Vector2 GetArcDirection()
    {
        float halfArc = arcAngle * 0.5f;
        float randomAngle = Random.Range(-halfArc, halfArc);

        float baseAngle = 0f;

        if (spawnInFrontOfPlayer && playerController != null)
        {
            Vector2 moveDir = playerController.PlayerDirection;
            if (moveDir != Vector2.zero)
                baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        }

        float rad = (baseAngle + randomAngle) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}

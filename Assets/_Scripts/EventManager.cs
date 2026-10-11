using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    [Header("Special Events Configuration")]
    [SerializeField] private List<EventData> eventDatas = new();
    [SerializeField] public float eventInterval = 45f;
    [SerializeField] private float eliteSpawnInterval = 75f;
    public float eventTimer = 25f;

    private Transform player;
    private float intervalTimer;
    private float activeEventTimer;
    private float eliteTimer;
    private bool isEventActive;

    private readonly List<(string poolKey, GameObject enemyObj)> spawnedEventEnemies = new();

    private void Awake()
    {
        instance = this;
        GameObject playerObj = GameObject.FindGameObjectWithTag(CONSTANT.PLAYER_TAG);
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null) return;

        // 1. Bộ đếm xuất hiện Quái Tinh Anh (Elite rơi Rương) định kỳ
        eliteTimer += Time.deltaTime;
        if (eliteTimer >= eliteSpawnInterval)
        {
            eliteTimer = 0f;
            SpawnEliteHunter(1, 8f);
        }

        // 2. Bộ đếm kích hoạt Special Wave Event
        if (!isEventActive)
        {
            intervalTimer += Time.deltaTime;
            if (intervalTimer >= eventInterval)
            {
                intervalTimer = 0f;
                PickRandomEvent();
            }
        }
        else
        {
            activeEventTimer += Time.deltaTime;
            if (activeEventTimer >= eventTimer)
            {
                EndEvent();
            }
        }
    }

    public void PickRandomEvent()
    {
        if (eventDatas == null || eventDatas.Count == 0)
            return;

        // Nếu trong Scene mới chỉ kéo 1 EventData (CirleEvent), xen kẽ thêm SwarmStampede và SniperSquad để đa dạng lối chơi
        if (eventDatas.Count == 1)
        {
            int roll = Random.Range(0, 3);
            if (roll == 0)
            {
                PlayEvent(eventDatas[0]);
            }
            else if (roll == 1)
            {
                eventTimer = 15f;
                isEventActive = true;
                activeEventTimer = 0f;
                SpawnSwarmStampede("Slime_water", 20, 9f, 1.8f);
                EventBus<OnSpecialEventTriggered>.Publish(new OnSpecialEventTriggered("Swarm Stampede", SpecialEventType.SwarmStampede));
            }
            else
            {
                eventTimer = 25f;
                isEventActive = true;
                activeEventTimer = 0f;
                SpawnSniperSquad("Slime_green", 6, 8f);
                EventBus<OnSpecialEventTriggered>.Publish(new OnSpecialEventTriggered("Sniper Squad", SpecialEventType.SniperSquad));
            }
            return;
        }

        // Chọn ngẫu nhiên theo trọng số probability
        float totalWeight = 0f;
        foreach (var data in eventDatas)
        {
            if (data != null)
                totalWeight += Mathf.Max(1f, data.probability);
        }

        float rand = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var data in eventDatas)
        {
            if (data == null) continue;
            cumulative += Mathf.Max(1f, data.probability);
            if (rand <= cumulative)
            {
                PlayEvent(data);
                return;
            }
        }

        PlayEvent(eventDatas[0]);
    }

    public void PlayEvent(EventData eventData)
    {
        if (eventData == null || player == null)
            return;

        eventTimer = eventData.eventDuration > 0f ? eventData.eventDuration : 25f;
        activeEventTimer = 0f;
        isEventActive = true;

        float radius = eventData.spawnRadius > 0f ? eventData.spawnRadius : 7.5f;
        int count = Mathf.Max(1, eventData.spawnCount);
        string key = ResolvePoolKey(eventData);

        switch (eventData.eventType)
        {
            case SpecialEventType.RingAmbush:
                SpawnRingAmbush(key, count, radius);
                break;

            case SpecialEventType.SwarmStampede:
                SpawnSwarmStampede(key, count, radius + 1.5f, eventData.stampedeSpeedMultiplier);
                break;

            case SpecialEventType.EliteHunter:
                SpawnEliteHunter(count, radius);
                break;

            case SpecialEventType.SniperSquad:
                SpawnSniperSquad(key, Mathf.Max(4, count), radius);
                break;
        }

        EventBus<OnSpecialEventTriggered>.Publish(new OnSpecialEventTriggered(eventData.eventName, eventData.eventType));
    }

    private string ResolvePoolKey(EventData data)
    {
        if (!string.IsNullOrEmpty(data.prefabkey) && PoolManager.Instance.HasKey(data.prefabkey))
            return data.prefabkey;

        return data.eventType switch
        {
            SpecialEventType.RingAmbush => "Slime_Stone",
            SpecialEventType.SwarmStampede => "Slime_water",
            SpecialEventType.EliteHunter => "Elite",
            SpecialEventType.SniperSquad => "Slime_green",
            _ => "Slime_Stone"
        };
    }

    // 1. VÒNG VÂY SIẾT CHẶT (RING AMBUSH)
    private void SpawnRingAmbush(string poolKey, int count, float radius)
    {
        float angleStep = 360f / Mathf.Max(1, count);
        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            Vector3 spawnPos = GetPositionOnCircle(angle, radius);
            SpawnAndTrackEventEnemy(poolKey, spawnPos);
        }
    }

    // 2. ĐÀN QUÁI ÙA QUA MÀN HÌNH (SWARM STAMPEDE)
    private void SpawnSwarmStampede(string poolKey, int count, float spawnDist, float speedMultiplier)
    {
        float startAngle = Random.Range(0f, 360f);
        Vector3 clusterCenter = GetPositionOnCircle(startAngle, spawnDist);
        Vector2 chargeDirection = ((Vector2)player.position - (Vector2)clusterCenter).normalized;
        Vector2 perp = new Vector2(-chargeDirection.y, chargeDirection.x);

        int rows = Mathf.CeilToInt(Mathf.Sqrt(count));
        int cols = Mathf.CeilToInt((float)count / rows);
        float spacing = 0.65f;

        int spawned = 0;
        for (int r = 0; r < rows && spawned < count; r++)
        {
            for (int c = 0; c < cols && spawned < count; c++)
            {
                float offsetPerp = (c - (cols - 1) * 0.5f) * spacing;
                float offsetBack = -r * spacing;
                Vector3 pos = clusterCenter + (Vector3)(perp * offsetPerp + chargeDirection * offsetBack);

                GameObject go = SpawnAndTrackEventEnemy(poolKey, pos);
                if (go != null && go.TryGetComponent<SlimeMovementBase>(out var move))
                {
                    move.EnableStampede(chargeDirection, speedMultiplier);
                }
                spawned++;
            }
        }
    }

    // 3. QUÁI TINH ANH RƠI RƯƠNG (ELITE HUNTER)
    private void SpawnEliteHunter(int count, float radius)
    {
        string key = PoolManager.Instance.HasKey("Elite") ? "Elite" : "Slime_Dark";
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 pos = GetPositionOnCircle(angle, radius);
            GameObject eliteObj = PoolManager.Instance.Spawn(key, pos);
            if (eliteObj != null)
            {
                if (eliteObj.TryGetComponent<Slime_Base>(out var slime))
                {
                    slime.IsElite = true;
                    if (EnemySpawner.Instance != null)
                        EnemySpawner.Instance.RegisterEnemy(slime);
                }
            }
        }
    }

    // 4. ĐỘI HÌNH BẮN TỈA 4 GÓC (SNIPER SQUAD)
    private void SpawnSniperSquad(string poolKey, int count, float radius)
    {
        string key = PoolManager.Instance.HasKey(poolKey) ? poolKey : "Slime_green";
        float angleStep = 360f / Mathf.Max(1, count);
        for (int i = 0; i < count; i++)
        {
            float angle = 45f + angleStep * i;
            Vector3 pos = GetPositionOnCircle(angle, radius);
            SpawnAndTrackEventEnemy(key, pos);
        }
    }

    private GameObject SpawnAndTrackEventEnemy(string poolKey, Vector3 pos)
    {
        if (!PoolManager.Instance.HasKey(poolKey)) return null;
        GameObject enemy = PoolManager.Instance.Spawn(poolKey, pos);
        if (enemy != null)
        {
            spawnedEventEnemies.Add((poolKey, enemy));
        }
        return enemy;
    }

    public void EndEvent()
    {
        isEventActive = false;
        activeEventTimer = 0f;

        // Chỉ thu hồi đúng những con quái do Event tạo ra, không xóa nhầm quái thường hoặc Boss
        for (int i = 0; i < spawnedEventEnemies.Count; i++)
        {
            var (poolKey, enemyObj) = spawnedEventEnemies[i];
            if (enemyObj != null && enemyObj.activeInHierarchy)
            {
                PoolManager.Instance.Despawn(poolKey, enemyObj);
            }
        }
        spawnedEventEnemies.Clear();
    }

    private Vector3 GetPositionOnCircle(float angleDeg, float radius = 7f)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float x = player.position.x + Mathf.Cos(rad) * radius;
        float y = player.position.y + Mathf.Sin(rad) * radius;
        return new Vector3(x, y, 0f);
    }
}

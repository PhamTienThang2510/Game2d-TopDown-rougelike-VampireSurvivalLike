using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private float offsetY = 0.3f;

    private void OnEnable()
    {
        EventBus<OnEnemyKilledEvent>.Subscribe(OnEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<OnEnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
    }

    private void OnEnemyKilled(OnEnemyKilledEvent evt)
    {
        Vector3 spawnPos = evt.Position + Vector2.up * offsetY;
        PoolManager.Instance.Spawn(CONSTANT.Gem_exp, spawnPos);

        if (evt.DropTreasure)
        {
            SpawnTreasure(evt.Position);
        }
    }

    private void SpawnTreasure(Vector2 position)
    {
        if (treasurePrefab != null)
        {
            Instantiate(treasurePrefab, position, Quaternion.identity);
            return;
        }

        // Fallback tạo Rương nâng cấp trực tiếp nếu chưa kéo treasurePrefab vào Inspector
        GameObject chest = new GameObject("TreasureChest_Drop");
        chest.transform.position = position;
        CircleCollider2D col = chest.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.6f;
        chest.AddComponent<Treasure>();
    }
}

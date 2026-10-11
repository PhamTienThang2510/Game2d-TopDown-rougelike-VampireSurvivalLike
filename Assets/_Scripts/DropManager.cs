using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
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
    }
}

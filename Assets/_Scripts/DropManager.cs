using UnityEngine;

public class DropManager : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private float offsetY = 0.3f;

    private void OnEnable()
    {
        Slime.OnSlimeDie += SpawnDrop;
    }

    private void OnDisable()
    {
        Slime.OnSlimeDie -= SpawnDrop;
    }

    private void SpawnDrop(Vector2 enemyPos)
    {
        Vector3 spawnPos = enemyPos + Vector2.up * offsetY;
        PoolManager.Instance.Spawn(CONSTANT.Gem_exp, spawnPos);
    }
}

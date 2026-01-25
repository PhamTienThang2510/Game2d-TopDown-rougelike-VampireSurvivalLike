using UnityEngine;
using System.Collections;

public class EffectDespawn : Despawn
{
    [SerializeField] private float despawnTime = 0.5f;

    private void Reset()
    {
        despawnTime = 0.5f;
    }

    private void Awake()
    {
    }

    private void OnEnable()
    {
        // Bắt đầu đếm thời gian despawn
        StartCoroutine(DespawnAfter());
    }

    private IEnumerator DespawnAfter()
    {
        yield return new WaitForSeconds(despawnTime);
        DespawnObj();
    }

    protected override void DespawnObj()
    {
        if (!canDespawn) return;

        // Dùng poolKey đảm bảo đúng key despawn
        PoolManager.Instance.Despawn(this.name, this.gameObject);
    }
}

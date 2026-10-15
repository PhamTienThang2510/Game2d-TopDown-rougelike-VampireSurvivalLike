using System.Collections.Generic;
using UnityEngine;

public class LightningController : WeaponBase, IWeaponUI
{
    [Header("Lightning Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float searchRadius = 6f;
    [SerializeField] private int baseMaxChain = 3;

    protected override void Attack()
    {
        float radius = searchRadius * GetFinalAreaScale();
        int initialStrikes = GetFinalProjectileCount();
        int chainLimit = baseMaxChain + (currentLevel - 1);

        List<IDamageable> allHit = new();

        for (int i = 0; i < initialStrikes; i++)
        {
            IDamageable target = FindNearestEnemy(spawnHolder.position, radius, allHit);
            if (target == null) break;

            ChainLightning(target, radius, chainLimit, allHit);
        }
    }

    private void ChainLightning(IDamageable firstTarget, float radius, int maxChain, List<IDamageable> globalHit)
    {
        IDamageable current = firstTarget;
        int hitLeft = maxChain + 1;
        int damage = GetFinalDamage();

        while (current != null && hitLeft > 0)
        {
            globalHit.Add(current);
            current.TakeDamage(damage);

            if (current is Component comp)
            {
                SpawnLightningFX(comp.transform.position);
                current = FindNearestEnemy(comp.transform.position, radius, globalHit);
            }
            else
            {
                break;
            }

            hitLeft--;
        }
    }

    private IDamageable FindNearestEnemy(Vector2 center, float radius, List<IDamageable> ignore = null)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        IDamageable nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable target)) continue;
            if (ignore != null && ignore.Contains(target)) continue;

            float dist = Vector2.Distance(center, target.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = target;
            }
        }

        return nearest;
    }

    private void SpawnLightningFX(Vector2 targetPos)
    {
        if (PoolManager.Instance != null && PoolManager.Instance.HasKey(CONSTANT.Fx_Lightning))
        {
            GameObject fx = PoolManager.Instance.Spawn(CONSTANT.Fx_Lightning, targetPos);
            if (fx != null)
            {
                fx.transform.position = targetPos;
                fx.transform.localScale = Vector3.one * GetFinalAreaScale();
                fx.GetComponent<LightningFX>()?.Play();
            }
        }
    }

    // ================= TƯƠNG THÍCH NGƯỢC IWeaponUI =================
    public SpriteRenderer GetIcon()
    {
        return weaponData != null ? weaponData.weaponSprite : null;
    }

    public void UpdateWeaponData(int addDame)
    {
        UpgradeWeapon();
    }
}

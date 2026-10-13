using System.Collections.Generic;
using UnityEngine;

public class MagicOrbWeapon : WeaponController, IWeaponUI
{
    [SerializeField] private float baseRange = 6f;

    protected override void Attack()
    {
        if (weaponData == null || !PoolManager.Instance.HasKey(weaponData.projectileKey))
            return;

        float effectiveRange = baseRange * GetFinalAreaScale();
        List<Transform> targets = GetEnemiesInRange(effectiveRange);

        if (targets.Count == 0) return;

        int count = GetFinalProjectileCount();

        for (int i = 0; i < count; i++)
        {
            // Phân bổ mục tiêu: nếu nhiều quái thì chia đều, nếu 1 quái thì dồn toàn bộ đạn vào nó
            Transform target = targets[i % targets.Count];
            SpawnOrb(target);
        }
    }

    private void SpawnOrb(Transform target)
    {
        GameObject obj = PoolManager.Instance.Spawn(weaponData.projectileKey, spawnHolder.position);
        if (obj == null) return;

        obj.transform.localScale = Vector3.one * GetFinalAreaScale();

        if (obj.TryGetComponent(out MagicOrbProjectile orb))
        {
            orb.Initialize(weaponData.projectileKey, GetFinalDamage(), GetFinalSpeed(), target);
        }
        else if (obj.TryGetComponent(out Projectile proj))
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)spawnHolder.position).normalized;
            proj.InitializeAdvanced(
                key: weaponData.projectileKey,
                dame: GetFinalDamage(),
                speed: GetFinalSpeed(),
                dir: dir,
                motion: ProjectileMotionType.Homing,
                areaScale: GetFinalAreaScale(),
                target: target
            );
        }
    }

    private List<Transform> GetEnemiesInRange(float range)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(CONSTANT.ENEMY_TAG);
        List<Transform> result = new();
        Vector2 playerPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float dist = Vector2.Distance(playerPos, enemy.transform.position);
            if (dist <= range)
            {
                result.Add(enemy.transform);
            }
        }

        // Sắp xếp theo khoảng cách gần nhất
        result.Sort((a, b) =>
            Vector2.Distance(playerPos, a.position).CompareTo(Vector2.Distance(playerPos, b.position))
        );

        return result;
    }

    // ================= TƯƠNG THÍCH NGƯỢC IWeaponUI =================
    public SpriteRenderer GetIcon()
    {
        return weaponData != null ? weaponData.weaponSprite : null;
    }

    public void UpdateWeaponData(int AddDame)
    {
        UpgradeWeapon();
    }
}

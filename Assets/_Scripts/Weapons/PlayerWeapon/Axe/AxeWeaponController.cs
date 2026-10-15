using UnityEngine;

public class AxeWeaponController : WeaponBase, IWeaponUI
{
    protected override void Attack()
    {
        if (weaponData == null || !PoolManager.Instance.HasKey(weaponData.projectileKey))
            return;

        Transform target = GetNearestEnemy();
        Vector2 baseDir = (target != null)
            ? ((Vector2)target.position - (Vector2)transform.position).normalized
            : GetRandomDirection();

        int count = GetFinalProjectileCount();
        float angleStep = (count > 1) ? (360f / count) : 0f;

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = (count == 1)
                ? baseDir
                : (Vector2)(Quaternion.Euler(0, 0, i * angleStep) * baseDir);

            SpawnAxe(dir);
        }
    }

    private void SpawnAxe(Vector2 dir)
    {
        GameObject obj = PoolManager.Instance.Spawn(weaponData.projectileKey, spawnHolder.position);
        if (obj == null) return;

        // Scale kích thước rìu theo AreaScale
        obj.transform.localScale = Vector3.one * GetFinalAreaScale();

        // Tương thích cả BoomerangProjectile cũ và Projectile mới
        if (obj.TryGetComponent(out BoomerangProjectile boomerang))
        {
            boomerang.Initialize(weaponData.projectileKey, GetFinalDamage(), GetFinalSpeed(), dir, transform);
        }
        else if (obj.TryGetComponent(out Projectile proj))
        {
            proj.InitializeAdvanced(
                key: weaponData.projectileKey,
                dame: GetFinalDamage(),
                speed: GetFinalSpeed(),
                dir: dir,
                motion: ProjectileMotionType.Boomerang,
                areaScale: GetFinalAreaScale(),
                spinSpeed: currentSpinSpeed,
                ownerTransform: transform
            );
        }
    }

    private Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(CONSTANT.ENEMY_TAG);
        Transform nearest = null;
        float minDistance = float.MaxValue;
        Vector2 playerPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float dist = Vector2.Distance(playerPos, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
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

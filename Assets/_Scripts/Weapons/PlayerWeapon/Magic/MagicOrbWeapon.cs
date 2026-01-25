using UnityEngine;

public class MagicOrbWeapon : WeaponController, IWeaponUI
{
    float timer = 0f;
    [SerializeField] float range = 5f;
    private void Awake()
    {
        this.dame = weaponData.damage;
        this.speed = weaponData.projectileSpeed;
        this.cooldown = weaponData.cooldown;
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            Fire();
            timer = 0f;
        }
    }

    void Fire()
    {
        if (!PoolManager.Instance.HasKey(weaponData.projectileKey))
            return;

        Transform target = GetNearestEnemyInRange();

        // Không có enemy → không bắn
        if (target == null) return;

        GameObject obj = PoolManager.Instance.Spawn(
            weaponData.projectileKey,
            transform.position
        );

        obj.GetComponent<MagicOrbProjectile>()
           .Initialize(weaponData.projectileKey, dame, speed, target);
    }

    Transform GetNearestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(CONSTANT.ENEMY_TAG);

        Transform nearest = null;
        float minDistance = range;
        Vector2 playerPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(playerPos, enemy.transform.position);

            if (dist <= range && dist < minDistance)
            {
                minDistance = dist;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }

    public SpriteRenderer GetIcon()
    {
        return weaponData.weaponSprite;
    }

    public void UpdateWeaponData(int AddDame)
    {
        this.dame += AddDame;
    }
}

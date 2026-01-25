using UnityEngine;

public class AxeWeaponController : WeaponController, IWeaponUI
{
    float timer;
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

        Transform target = GetNearestEnemy();

        Vector2 dir;

        if (target != null)
            dir = (target.position - transform.position).normalized;
        else
            dir = GetRandomDirection(); // fallback


        GameObject obj = PoolManager.Instance.Spawn(
            weaponData.projectileKey,
            transform.position
        );

        obj.GetComponent<BoomerangProjectile>()
           .Initialize(weaponData.projectileKey,dame,speed, dir, transform);
    }

    Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    Transform GetNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(CONSTANT.ENEMY_TAG);

        Transform nearest = null;
        float minDistance = float.MaxValue;
        Vector2 playerPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(playerPos, enemy.transform.position);
            if (dist < minDistance)
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

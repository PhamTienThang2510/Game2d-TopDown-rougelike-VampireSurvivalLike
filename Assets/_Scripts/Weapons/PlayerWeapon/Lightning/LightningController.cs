using System.Collections.Generic;
using UnityEngine;

public class LightningController : WeaponController, IWeaponUI
{
    private PlayerController player;

    [Header("Lightning Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float searchRadius = 6f;
    [SerializeField] private int maxChain = 3;

    private float timer;

    private void Awake()
    {
        dame = weaponData.damage;
        cooldown = weaponData.cooldown;
    }

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= cooldown)
        {
            timer = 0f;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (player == null) return;

        IDamageable firstTarget = FindNearestEnemy(player.transform.position);
        if (firstTarget == null) return;

        List<IDamageable> hitted = new();
        IDamageable current = firstTarget;

        int hitLeft = maxChain + 1;

        while (current != null && hitLeft > 0)
        {
            hitted.Add(current);
            current.TakeDamage(dame);

            SpawnLightningFX(current.transform.position);

            current = FindNearestEnemy(
                current.transform.position,
                hitted
            );

            hitLeft--;
        }
    }

    private IDamageable FindNearestEnemy(
        Vector2 center,
        List<IDamageable> ignore = null)
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(center, searchRadius, enemyLayer);

        IDamageable nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable target))
                continue;

            if (ignore != null && ignore.Contains(target))
                continue;

            float dist =
                Vector2.Distance(center, target.transform.position);

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
        GameObject fx =
            PoolManager.Instance.Spawn(CONSTANT.Fx_Lightning, targetPos);

        if (fx != null)
        {
            fx.transform.position = targetPos;
            fx.GetComponent<LightningFX>()?.Play();
        }
    }

    // ================= UI =================
    public SpriteRenderer GetIcon()
    {
        return weaponData.weaponSprite;
    }

    public void UpdateWeaponData(int addDame)
    {
        dame += addDame;
        maxChain += 1;
    }
}

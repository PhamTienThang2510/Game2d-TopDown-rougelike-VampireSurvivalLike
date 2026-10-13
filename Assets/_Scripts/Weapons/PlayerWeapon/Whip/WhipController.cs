using UnityEngine;
using static CONSTANT;

public class WhipController : WeaponController, IWeaponUI
{
    [Header("Whip Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector2 hitBoxSize = new(3f, 1.5f);
    [SerializeField] private float hitOffset = 1.7f;

    protected override void Attack()
    {
        Vector2 forwardDir = GetAimDirection();

        // 1. Quất đòn phía trước
        PerformWhipSlash(forwardDir);

        // 2. Nếu có từ 2 đòn trở lên: Quất thêm đòn bọc hậu phía sau lưng!
        if (GetFinalProjectileCount() >= 2)
        {
            PerformWhipSlash(-forwardDir);
        }
    }

    private void PerformWhipSlash(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float scaledOffset = hitOffset * GetFinalAreaScale();
        Vector2 center = (Vector2)spawnHolder.position + dir.normalized * scaledOffset;
        Vector2 scaledBox = hitBoxSize * GetFinalAreaScale();

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            center,
            scaledBox,
            angle,
            enemyLayer
        );

        int damage = GetFinalDamage();
        foreach (var hit in hitEnemies)
        {
            if (hit != null && hit.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(damage);
            }
        }

        SpawnWhipFX(center, angle);
    }

    private void SpawnWhipFX(Vector2 pos, float angle)
    {
        if (PoolManager.Instance != null && PoolManager.Instance.HasKey("WhipSlash"))
        {
            GameObject fx = PoolManager.Instance.Spawn("WhipSlash", pos);
            if (fx != null)
            {
                fx.transform.rotation = Quaternion.Euler(0, 0, angle);
                fx.transform.localScale = Vector3.one * GetFinalAreaScale();
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 dir = (playerInput != null && playerInput.LastMoveDirection.sqrMagnitude > 0.001f)
            ? playerInput.LastMoveDirection.normalized
            : Vector2.right;

        Transform holder = spawnHolder != null ? spawnHolder : transform;
        Vector2 center = (Vector2)holder.position + dir * hitOffset * GetFinalAreaScale();
        Vector2 scaledBox = hitBoxSize * GetFinalAreaScale();

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            center,
            Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg),
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, scaledBox);
    }
#endif
}

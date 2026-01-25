using UnityEngine;
using static CONSTANT;

public class WhipController : WeaponController, IWeaponUI
{
    private PlayerController player;

    [Header("Whip Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector2 hitBoxSize = new(3f, 1.5f);
    [SerializeField] private float hitOffset = 1.7f;

    private float timer;

    private void Awake()
    {
        dame = weaponData.damage;
        speed = weaponData.projectileSpeed;
        cooldown = weaponData.cooldown;
    }

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    void Update()
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

        Vector2 dir = player.LastMoveDirection;
        if (dir == Vector2.zero)
            dir = Vector2.right;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector2 center = (Vector2)spawnHolder.transform.position + dir.normalized * hitOffset;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(
            center,
            hitBoxSize,
            angle,
            enemyLayer
        );

        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(dame);
            }
        }

        // Visual (optional)
        SpawnWhipFX(center, angle);
    }

    private void SpawnWhipFX(Vector2 pos, float angle)
    {
        GameObject fx = PoolManager.Instance.Spawn("WhipSlash", pos);
        if (fx != null)
        {
            fx.transform.rotation = Quaternion.Euler(0, 0, angle);
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
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Vector2 dir = player.LastMoveDirection;
        Vector2 center = (Vector2)spawnHolder.transform.position + dir.normalized * hitOffset;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            center,
            Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg),
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, hitBoxSize);
    }
#endif
}

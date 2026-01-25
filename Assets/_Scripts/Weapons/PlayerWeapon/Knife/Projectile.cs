using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private int damage;
    [Header("Despawn settings")]
    [SerializeField] private float viewportMargin = 0.2f;
    string key;
    private Camera mainCam;
    private void Awake()
    {
        mainCam = Camera.main;
    }
    public void Initialize(string key,int dame, float speed, Vector2 dir)
    {
        direction = dir.normalized;
        this.speed = speed;
        this.damage = dame;
        this.key = key;
        // Quay projectile về hướng bắn
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void Update()
    {
        // Chỉ di chuyển nếu speed > 0
        if (speed > 0)
            transform.position += (Vector3)direction * speed * Time.deltaTime;
        CheckOutOfCamera();
    }
    private void CheckOutOfCamera()
    {
        if (mainCam == null) return;

        Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);

        if (viewportPos.x < -viewportMargin ||
            viewportPos.x > 1 + viewportMargin ||
            viewportPos.y < -viewportMargin ||
            viewportPos.y > 1 + viewportMargin)
        {
            // Chỉ despawn nếu object nằm trong Pool
            if (PoolManager.Instance != null)
            {
                if (PoolManager.Instance.HasKey(key))
                {
                    PoolManager.Instance.Despawn(key, gameObject);
                }
            }
        }
    }
    private void HandleDamage(GameObject target)
    {
        if (!target.CompareTag(CONSTANT.ENEMY_TAG)) return;

        var slime = target.GetComponent<IDamageable>();
        if (slime != null)
        {
            slime.TakeDamage(damage);
        }

        // Chỉ despawn nếu object nằm trong Pool
        if (PoolManager.Instance != null)
        {
            if (PoolManager.Instance.HasKey(key))
            {
                PoolManager.Instance.Despawn(key, gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleDamage(other.gameObject);
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy_Attack : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string poolKey = "Slime_Attack";
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;

    private Vector2 direction;
    private float timer;

    private void OnEnable()
    {
        timer = 0f;
    }

    private void Update()
    {
        Move();
        CheckLifeTime();
    }

    private void Move()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void CheckLifeTime()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
            Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(CONSTANT.PLAYER_TAG)) return;

        if (other.TryGetComponent(out Player player))
        {
            player.TakeDamage(damage);
        }

        Despawn();
    }

    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Despawn()
    {
        PoolManager.Instance?.Despawn(poolKey, gameObject);
    }
}

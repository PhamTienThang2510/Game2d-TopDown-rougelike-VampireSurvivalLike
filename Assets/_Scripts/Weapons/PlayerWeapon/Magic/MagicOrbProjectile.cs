using JetBrains.Annotations;
using UnityEngine;

public class MagicOrbProjectile : MonoBehaviour
{
    Transform target;
    Vector2 direction;
    float lifeTime = 3f;
    float timer;

    string poolKey;
    int dame;
    float speed;
    public void Initialize(string key, int dame, float speed, Transform targetEnemy)
    {
        target = targetEnemy;
        poolKey = key;
        this.dame = dame;
        this.speed = speed;
        timer = 0f;

        if (target != null)
            direction = (target.position - transform.position).normalized;

        RotateTowards(direction);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Nếu target còn sống → cập nhật hướng liên tục (homing)
        if (target != null && target.gameObject.activeInHierarchy)
        {
            direction = (target.position - transform.position).normalized;
        }

        // XOAY MẶT VỀ HƯỚNG BAY
        RotateTowards(direction);

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (timer >= lifeTime)
            Despawn();
    }

    void RotateTowards(Vector2 dir)
    {
        if (dir == Vector2.zero) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

    }

    void Despawn()
    {
        PoolManager.Instance.Despawn(poolKey, gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(CONSTANT.ENEMY_TAG))
        {
            collision.GetComponent<IDamageable>()?.TakeDamage(dame);
            Despawn();
        }
    }
}

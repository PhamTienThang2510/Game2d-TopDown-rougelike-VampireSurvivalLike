using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    Transform owner;
    Vector2 direction;
    Vector2 startPos;
    bool isReturning;
    float timer;
    float maxDistance = 4.5f;
    float lifeTime = 4f;
    // PARABOL CONFIG
    float curveStrength = 1.5f;
    float curveFrequency = 6f;
    // ROTATION
    float rotationSpeed = 600f;

    string poolKey;
    int dame;
    float speed;

    public void Initialize(string key,int dame, float speed, Vector2 dir, Transform ownerTransform)
    {
        owner = ownerTransform;
        poolKey = key;
        this.dame = dame;
        this.speed = speed;
        direction = dir.normalized;
        startPos = transform.position;

        isReturning = false;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // XOAY TRÒN
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (!isReturning)
        {
            MoveParabol(direction, speed);

            if (Vector2.Distance(startPos, transform.position) >= maxDistance)
                isReturning = true;
        }
        else
        {
            Vector2 returnDir = (owner.position - transform.position).normalized;
            MoveParabol(returnDir, speed * 1.2f);

            if (Vector2.Distance(owner.position, transform.position) < 0.3f)
                Despawn();
        }

        if (timer >= lifeTime)
            Despawn();
    }

    void MoveParabol(Vector2 moveDir, float speed)
    {
        Vector2 forward = moveDir * speed * Time.deltaTime;

        Vector2 perpendicular = new Vector2(-moveDir.y, moveDir.x);

        float curveOffset = Mathf.Sin(timer * curveFrequency) * curveStrength * Time.deltaTime;

        Vector2 curvedMove = forward + perpendicular * curveOffset;

        transform.position += (Vector3)curvedMove;
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
        }
    }

}

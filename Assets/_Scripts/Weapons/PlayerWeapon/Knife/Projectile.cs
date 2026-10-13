using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Runtime State")]
    private Vector2 direction;
    private Vector2 baseDirection;
    private Vector2 perpendicular;
    private Vector2 startPos;
    private float speed;
    private int damage;
    private string key;

    [Header("Motion Configuration")]
    private ProjectileMotionType motionType = ProjectileMotionType.Straight;
    private float spinSpeed = 720f;
    private float waveFrequency = 10f;
    private float waveAmplitude = 1.2f;
    private float timer = 0f;
    private float lifeTime = 5f;
    private bool isReturning = false;
    private float maxBoomerangDistance = 5f;

    [Header("References")]
    private Transform owner;
    private Transform targetEnemy;
    private Camera mainCam;

    [Header("Despawn settings")]
    [SerializeField] private float viewportMargin = 0.2f;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    /// <summary>
    /// Hàm khởi tạo cũ (Giữ lại để tương thích 100% với code hiện có)
    /// </summary>
    public void Initialize(string key, int dame, float speed, Vector2 dir)
    {
        InitializeAdvanced(
            key: key,
            dame: dame,
            speed: speed,
            dir: dir,
            motion: ProjectileMotionType.Straight,
            areaScale: 1f
        );
    }

    /// <summary>
    /// Hàm khởi tạo nâng cao hỗ trợ đầy đủ mọi kiểu quỹ đạo chuyển động linh hoạt
    /// </summary>
    public void InitializeAdvanced(
        string key,
        int dame,
        float speed,
        Vector2 dir,
        ProjectileMotionType motion = ProjectileMotionType.Straight,
        float areaScale = 1f,
        float spinSpeed = 720f,
        float waveFrequency = 10f,
        float waveAmplitude = 1.2f,
        Transform ownerTransform = null,
        Transform target = null,
        float customLifeTime = 5f)
    {
        this.key = key;
        this.damage = dame;
        this.speed = speed;
        this.direction = (dir.sqrMagnitude < 0.001f) ? Vector2.right : dir.normalized;
        this.baseDirection = this.direction;
        this.perpendicular = new Vector2(-this.direction.y, this.direction.x);
        this.startPos = transform.position;

        this.motionType = motion;
        this.spinSpeed = spinSpeed;
        this.waveFrequency = waveFrequency;
        this.waveAmplitude = waveAmplitude;
        this.owner = ownerTransform;
        this.targetEnemy = target;
        this.lifeTime = customLifeTime;

        this.timer = 0f;
        this.isReturning = false;

        // Scale kích thước đạn theo Area
        transform.localScale = Vector3.one * Mathf.Max(0.1f, areaScale);

        // Hướng mặt ban đầu về hướng bắn
        UpdateRotationTowardsDirection(this.direction);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        UpdateMotion();

        // Tự động thu hồi nếu quá thời gian sống
        if (timer >= lifeTime)
        {
            DespawnSelf();
            return;
        }

        // Với đạn Boomerang hoặc bay quanh chủ, không tự despawn khi ra ngoài camera quá sớm
        if (motionType != ProjectileMotionType.Boomerang)
        {
            CheckOutOfCamera();
        }
    }

    private void UpdateMotion()
    {
        switch (motionType)
        {
            case ProjectileMotionType.Straight:
                transform.position += (Vector3)(direction * speed * Time.deltaTime);
                break;

            case ProjectileMotionType.Spinning:
                transform.position += (Vector3)(direction * speed * Time.deltaTime);
                transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
                break;

            case ProjectileMotionType.SineWave:
                Vector2 forwardMove = direction * speed * Time.deltaTime;
                float waveOffset = Mathf.Sin(timer * waveFrequency) * waveAmplitude * Time.deltaTime;
                transform.position += (Vector3)(forwardMove + perpendicular * waveOffset);
                break;

            case ProjectileMotionType.Boomerang:
                // Vừa bay vừa xoay tròn
                transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

                if (!isReturning)
                {
                    Vector2 forward = direction * speed * Time.deltaTime;
                    float curve = Mathf.Sin(timer * waveFrequency) * waveAmplitude * Time.deltaTime;
                    transform.position += (Vector3)(forward + perpendicular * curve);

                    if (Vector2.Distance(startPos, transform.position) >= maxBoomerangDistance)
                    {
                        isReturning = true;
                    }
                }
                else
                {
                    if (owner != null)
                    {
                        Vector2 returnDir = ((Vector2)owner.position - (Vector2)transform.position).normalized;
                        transform.position += (Vector3)(returnDir * speed * 1.3f * Time.deltaTime);

                        if (Vector2.Distance(owner.position, transform.position) < 0.4f)
                        {
                            DespawnSelf();
                        }
                    }
                    else
                    {
                        DespawnSelf();
                    }
                }
                break;

            case ProjectileMotionType.Homing:
                if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
                {
                    direction = ((Vector2)targetEnemy.position - (Vector2)transform.position).normalized;
                }
                UpdateRotationTowardsDirection(direction);
                transform.position += (Vector3)(direction * speed * Time.deltaTime);
                break;
        }
    }

    private void UpdateRotationTowardsDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
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
            DespawnSelf();
        }
    }

    private void HandleDamage(GameObject target)
    {
        if (!target.CompareTag(CONSTANT.ENEMY_TAG)) return;

        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        // Boomerang không biến mất khi chạm 1 quái (chém xuyên quái)
        if (motionType != ProjectileMotionType.Boomerang)
        {
            DespawnSelf();
        }
    }

    private void DespawnSelf()
    {
        if (PoolManager.Instance != null && !string.IsNullOrEmpty(key))
        {
            if (PoolManager.Instance.HasKey(key))
            {
                PoolManager.Instance.Despawn(key, gameObject);
                return;
            }
        }

        gameObject.SetActive(false);
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

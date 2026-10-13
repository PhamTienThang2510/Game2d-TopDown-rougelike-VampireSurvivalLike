using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponController : MonoBehaviour
{
    [Header("Data & Placement")]
    [SerializeField] public WeaponData weaponData;
    [SerializeField] public Transform spawnHolder;

    [Header("Runtime Stats (Level)")]
    public int currentLevel { get; protected set; } = 1;
    public int currentDamage { get; protected set; } = 1;
    public float currentSpeed { get; protected set; } = 10f;
    public float currentCooldown { get; protected set; } = 1f;
    public int currentProjectileCount { get; protected set; } = 1;
    public float currentAreaScale { get; protected set; } = 1f;
    public float currentDuration { get; protected set; } = 1f;

    [Header("Runtime Attack Pattern")]
    public FirePattern currentFirePattern { get; protected set; } = FirePattern.Forward;
    public ProjectileMotionType currentMotionType { get; protected set; } = ProjectileMotionType.Straight;
    public float currentSpreadAngle { get; protected set; } = 15f;
    public float currentSpinSpeed { get; protected set; } = 720f;
    public float currentWaveFrequency { get; protected set; } = 10f;
    public float currentWaveAmplitude { get; protected set; } = 1.2f;

    [Header("Tương thích ngược")]
    [SerializeField] public int dame;
    [SerializeField] public float speed;
    [SerializeField] public float cooldown;

    protected float timer = 0f;
    protected PlayerInput playerInput;

    /// <summary>
    /// Cho phép lớp con bật/tắt cơ chế tự động đếm giờ tấn công mặc định
    /// </summary>
    protected virtual bool AutoAttackEnabled => true;

    public int MaxLevel => weaponData != null ? weaponData.MaxLevel : 8;
    public bool IsMaxLevel => currentLevel >= MaxLevel;

    protected virtual void Awake()
    {
        if (spawnHolder == null) spawnHolder = transform;
        ApplyLevelStats(currentLevel);
    }

    protected virtual void Start()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    protected virtual void Update()
    {
        if (!AutoAttackEnabled) return;

        timer += Time.deltaTime;
        if (timer >= GetFinalCooldown())
        {
            timer = 0f;
            Attack();
        }
    }

    /// <summary>
    /// Hàm tấn công chính do từng loại vũ khí thực thi
    /// </summary>
    protected abstract void Attack();

    // ================= STAT CALCULATIONS (SẴN SÀNG CHO PASSIVE) =================
    public virtual int GetFinalDamage() => currentDamage;
    public virtual float GetFinalCooldown() => Mathf.Max(0.05f, currentCooldown);
    public virtual float GetFinalSpeed() => currentSpeed;
    public virtual int GetFinalProjectileCount() => Mathf.Max(1, currentProjectileCount);
    public virtual float GetFinalAreaScale() => Mathf.Max(0.1f, currentAreaScale);
    public virtual float GetFinalDuration() => Mathf.Max(0.1f, currentDuration);

    // ================= UPGRADE LOGIC =================
    public virtual void ApplyLevelStats(int level)
    {
        currentLevel = level;

        if (weaponData == null) return;

        WeaponLevelData lvlData = weaponData.GetLevelData(level);
        if (lvlData != null)
        {
            currentDamage = lvlData.damage;
            currentSpeed = lvlData.projectileSpeed;
            currentCooldown = lvlData.cooldown;
            currentProjectileCount = lvlData.projectileCount;
            currentAreaScale = lvlData.areaScale;
            currentDuration = lvlData.duration;

            currentFirePattern = lvlData.firePattern;
            currentMotionType = lvlData.motionType;
            currentSpreadAngle = lvlData.spreadAngle;
            currentSpinSpeed = lvlData.spinSpeed;
            currentWaveFrequency = lvlData.waveFrequency;
            currentWaveAmplitude = lvlData.waveAmplitude;

            // Đồng bộ với các biến cũ
            this.dame = currentDamage;
            this.speed = currentSpeed;
            this.cooldown = currentCooldown;
        }
    }

    public virtual void UpgradeWeapon()
    {
        if (IsMaxLevel) return;

        currentLevel++;
        ApplyLevelStats(currentLevel);
        OnLevelUp(currentLevel);
    }

    protected virtual void OnLevelUp(int newLevel) { }

    // ================= COMMON HELPERS =================
    protected virtual Vector2 GetAimDirection()
    {
        if (playerInput != null)
        {
            Vector2 dir = playerInput.LastMoveDirection;
            if (dir.sqrMagnitude > 0.001f)
                return dir.normalized;
        }
        return Vector2.right;
    }

    /// <summary>
    /// Bắn đạn linh động theo cấu hình hiện tại (Forward, FanSpread, Radial, Burst...)
    /// </summary>
    protected void ExecuteFirePattern(Vector2 baseDir, Transform targetEnemy = null)
    {
        if (weaponData == null || !PoolManager.Instance.HasKey(weaponData.projectileKey))
            return;

        int count = GetFinalProjectileCount();
        Vector2 origin = spawnHolder.position;

        switch (currentFirePattern)
        {
            case FirePattern.Forward:
                for (int i = 0; i < count; i++)
                {
                    SpawnSingleProjectile(origin, baseDir, targetEnemy);
                }
                break;

            case FirePattern.FanSpread:
                if (count == 1)
                {
                    SpawnSingleProjectile(origin, baseDir, targetEnemy);
                }
                else
                {
                    float totalSpread = (count - 1) * currentSpreadAngle;
                    float startAngle = -totalSpread / 2f;

                    for (int i = 0; i < count; i++)
                    {
                        float currentAngle = startAngle + i * currentSpreadAngle;
                        Vector2 spreadDir = Quaternion.Euler(0, 0, currentAngle) * baseDir;
                        SpawnSingleProjectile(origin, spreadDir, targetEnemy);
                    }
                }
                break;

            case FirePattern.Radial360:
                float step = 360f / count;
                for (int i = 0; i < count; i++)
                {
                    Vector2 radialDir = Quaternion.Euler(0, 0, i * step) * Vector2.right;
                    SpawnSingleProjectile(origin, radialDir, targetEnemy);
                }
                break;

            case FirePattern.Burst:
                StartCoroutine(BurstRoutine(origin, baseDir, count, targetEnemy));
                break;

            case FirePattern.TargetEnemy:
                SpawnSingleProjectile(origin, baseDir, targetEnemy);
                break;
        }
    }

    private IEnumerator BurstRoutine(Vector2 origin, Vector2 baseDir, int count, Transform targetEnemy)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSingleProjectile(spawnHolder.position, baseDir, targetEnemy);
            yield return new WaitForSeconds(0.08f);
        }
    }

    protected virtual GameObject SpawnSingleProjectile(Vector2 spawnPos, Vector2 shootDir, Transform target = null)
    {
        GameObject obj = PoolManager.Instance.Spawn(weaponData.projectileKey, spawnPos);
        if (obj != null)
        {
            var proj = obj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.InitializeAdvanced(
                    key: weaponData.projectileKey,
                    dame: GetFinalDamage(),
                    speed: GetFinalSpeed(),
                    dir: shootDir,
                    motion: currentMotionType,
                    areaScale: GetFinalAreaScale(),
                    spinSpeed: currentSpinSpeed,
                    waveFrequency: currentWaveFrequency,
                    waveAmplitude: currentWaveAmplitude,
                    ownerTransform: transform,
                    target: target,
                    customLifeTime: GetFinalDuration() > 1f ? GetFinalDuration() : 5f
                );
            }
        }
        return obj;
    }

    public virtual Sprite GetIconSprite()
    {
        return weaponData != null ? weaponData.GetIcon() : null;
    }

    public virtual string GetWeaponName()
    {
        return weaponData != null ? weaponData.weaponName : gameObject.name;
    }

    public virtual string GetNextUpgradeDescription()
    {
        return weaponData != null ? weaponData.GetUpgradeDescription(currentLevel + 1) : string.Empty;
    }
}

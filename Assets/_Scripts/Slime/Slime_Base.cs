using UnityEngine;

public class Slime_Base : MonoBehaviour, IDamageable, IDameSource
{
    protected SlimeMovementBase slimeMovement;
    protected SlimeAnimation slimeAnim;

    [Header("Settings")]
    [SerializeField] protected EnemySO enemySO;
    [SerializeField] protected int Health;
    [SerializeField] protected int MaxHealth;
    [SerializeField] protected int Damage;
    [Header("Damage Text Settings")]
    [SerializeField] protected Vector3 damageTextOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] protected bool isDead;

    public bool IsElite { get; set; }

    protected virtual void Awake()
    {
        slimeAnim = GetComponent<SlimeAnimation>();
        slimeMovement = GetComponent<SlimeMovementBase>();
        if (enemySO != null)
        {
            this.MaxHealth = enemySO.MaxHealth;
            this.Health = MaxHealth;
            this.Damage = enemySO.damage;
        }
    }

    protected virtual void OnEnable()
    {
        isDead = false;
        IsElite = gameObject.name.Contains("Elite") || this is Boss;
        if (enemySO != null)
        {
            this.MaxHealth = enemySO.MaxHealth;
            this.Health = MaxHealth;
            this.Damage = enemySO.damage;
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        this.Health -= damage;

        slimeAnim?.PlayDamageFlash();
        GameObject dameText = PoolManager.Instance.Spawn(CONSTANT.Fx_Damage_text, transform.position + damageTextOffset);
        dameText.GetComponent<DameTextAnimation>().SetDamage(damage);

        if (slimeMovement != null && slimeMovement.target != null)
        {
            Vector2 knockDir = (transform.position - slimeMovement.target.position);
            slimeMovement.Knockback(knockDir);
        }

        if (Health <= 0)
            SlimeDie();
    }

    public virtual int SendDame()
    {
        return this.Damage;
    }

    protected virtual void SlimeDie()
    {
        if (isDead) return;

        isDead = true;
        Vector3 pos = transform.position + new Vector3(0, 0.5f, 0);

        PoolManager.Instance.Spawn(CONSTANT.Fx_Damage_Die, pos);
        EventBus<OnEnemyKilledEvent>.Publish(new OnEnemyKilledEvent(transform.position, IsElite));
        slimeAnim?.PlayDeathFade(() =>
        {
            PoolManager.Instance.Despawn(this.transform.name, gameObject);
        });
    }

    public virtual void UpgradeDataSlime(int healthMultiplier, int damageMultiplier)
    {
        this.MaxHealth = this.MaxHealth * healthMultiplier;
        this.Health = this.MaxHealth;
        this.Damage = this.Damage * damageMultiplier;
    }

    protected virtual void OnDisable()
    {
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.UnregisterEnemy(this);
    }
}

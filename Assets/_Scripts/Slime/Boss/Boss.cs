using UnityEngine;

public class Boss : Slime_Base, IDamageable, IDameSource
{
    private SlimeAnimation slimeAnim;
    private void Awake()
    {
        slimeAnim = GetComponent<SlimeAnimation>();
        slimeMovement = GetComponent<SlimeMovementBase>();
        this.MaxHealth = enemySO.MaxHealth;
        this.Health = MaxHealth;
        this.Damage = enemySO.damage;
    }
    private void OnEnable()
    {
        isDead = false;
    }
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        this.Health -= damage;

        slimeAnim?.PlayDamageFlash();
        GameObject DameText = PoolManager.Instance.Spawn(CONSTANT.Fx_Damage_text, transform.position + damageTextOffset);
        DameText.GetComponent<DameTextAnimation>().SetDamage(damage);

        if (slimeMovement != null)
        {
            Vector2 knockDir = (transform.position - slimeMovement.target.position);
            slimeMovement.Knockback(knockDir);
        }

        if (Health <= 0)
            SlimeDie();
    }

    public int SendDame()
    {
        return this.Damage;
    }

    private void SlimeDie()
    {
        if (isDead) return;

        isDead = true;
        Vector3 pos = transform.position + new Vector3(0, 0.5f, 0);

        PoolManager.Instance.Spawn(CONSTANT.Fx_Damage_Die, pos);
        OnSlimeDie?.Invoke(transform.position);
        slimeAnim?.PlayDeathFade(() =>
        {
            PoolManager.Instance.Despawn(this.transform.name, gameObject);
        });
    }
    public override void UpgradeDataSlime(int healthMultiplier, int damageMultiplier)
    {
        this.MaxHealth = this.MaxHealth * healthMultiplier;
        this.Health = this.MaxHealth;
        this.Damage = this.Damage * damageMultiplier;
    }
}

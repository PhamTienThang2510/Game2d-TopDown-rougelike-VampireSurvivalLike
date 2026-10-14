using UnityEngine;

public class ShieldWeaponController : WeaponController, IWeaponUI
{
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private float baseDamageInterval = 0.5f;

    private GameObject shieldInstance;
    private ShieldAreaDamage areaDamage;
    private bool shieldActive;

    protected override bool AutoAttackEnabled => false;

    protected override void Start()
    {
        base.Start();

        if (shieldPrefab != null)
        {
            shieldInstance = Instantiate(shieldPrefab, spawnHolder.position, Quaternion.identity);
            shieldInstance.transform.SetParent(spawnHolder);
            areaDamage = shieldInstance.GetComponent<ShieldAreaDamage>();
            if (areaDamage == null)
            {
                areaDamage = shieldInstance.AddComponent<ShieldAreaDamage>();
            }
            areaDamage.Initialize(GetFinalDamage(), baseDamageInterval);

            UpdateShieldVisual();
            shieldInstance.SetActive(false);
        }
    }

    protected override void Update()
    {
        if (shieldInstance == null) return;

        shieldInstance.transform.position = spawnHolder.position;

        timer += Time.deltaTime;

        float activeDuration = GetFinalDuration();
        float cooldownTime = GetFinalCooldown();

        if (!shieldActive && timer >= cooldownTime)
        {
            ActivateShield();
        }

        if (shieldActive && timer >= cooldownTime + activeDuration)
        {
            DeactivateShield();
        }
    }

    protected override void Attack()
    {
        // Shield sử dụng chu kỳ bật/tắt riêng trong Update
    }

    private void ActivateShield()
    {
        shieldActive = true;
        shieldInstance.SetActive(true);
        timer = GetFinalCooldown();
    }

    private void DeactivateShield()
    {
        shieldActive = false;
        shieldInstance.SetActive(false);
        timer = 0f;
    }

    protected override void OnLevelUp(int newLevel)
    {
        UpdateShieldVisual();
        if (areaDamage != null)
        {
            areaDamage.UpdateDamage(GetFinalDamage());
        }
    }

    private void UpdateShieldVisual()
    {
        if (shieldInstance != null)
        {
            shieldInstance.transform.localScale = Vector3.one * GetFinalAreaScale();
        }
    }

    // ================= TƯƠNG THÍCH NGƯỢC IWeaponUI =================
    public SpriteRenderer GetIcon()
    {
        return weaponData != null ? weaponData.weaponSprite : null;
    }

    public void UpdateWeaponData(int addDamage)
    {
        UpgradeWeapon();
    }
}

using UnityEngine;

public class ShieldWeaponController : WeaponController, IWeaponUI
{
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private float shieldDuration = 1.0f;
    [SerializeField] private float damageInterval = 0.5f;

    private GameObject shieldInstance;
    private ShieldAreaDamage areaDamage;

    private float timer;
    private bool shieldActive;

    private void Awake()
    {
        dame = weaponData.damage;
        speed = weaponData.projectileSpeed;
        cooldown = weaponData.cooldown;
    }

    private void Start()
    {
        shieldInstance = Instantiate(shieldPrefab, spawnHolder.position, Quaternion.identity);
        shieldInstance.SetActive(false);

        areaDamage = shieldInstance.AddComponent<ShieldAreaDamage>();
        areaDamage.Initialize(dame, damageInterval);
    }

    private void Update()
    {
        if (shieldInstance != null)
            shieldInstance.transform.position = spawnHolder.position;

        timer += Time.deltaTime;

        if (!shieldActive && timer >= cooldown)
        {
            ActivateShield();
        }

        if (shieldActive && timer >= cooldown + shieldDuration)
        {
            DeactivateShield();
        }
    }

    private void ActivateShield()
    {
        shieldActive = true;
        shieldInstance.SetActive(true);
        timer = cooldown;
    }

    private void DeactivateShield()
    {
        shieldActive = false;
        shieldInstance.SetActive(false);
        timer = 0f;
    }

    public void UpdateWeaponData(int addDamage)
    {
        dame += addDamage;

        if (areaDamage != null)
            areaDamage.UpdateDamage(dame);
    }

    public SpriteRenderer GetIcon()
    {
        return weaponData.weaponSprite;
    }
}

using UnityEngine;

public class KnifeWeaponController : WeaponController, IWeaponUI
{
    private PlayerInput playerInput;
    private float timer;
    private void Awake()
    {
        this.dame = weaponData.damage;
        this.speed = weaponData.projectileSpeed;
        this.cooldown = weaponData.cooldown;
    }
    private void Start()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            timer = 0;
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector2 dir = playerInput.LastMoveDirection;
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;

        GameObject proj = PoolManager.Instance.Spawn(weaponData.projectileKey, spawnHolder.position);
        proj.GetComponent<Projectile>().Initialize(weaponData.projectileKey,dame, speed, dir);
    }

    public SpriteRenderer GetIcon()
    {
        return weaponData.weaponSprite;
    }

    public void UpdateWeaponData(int AddDame)
    {
        this.dame += AddDame;
    }
}

using UnityEngine;

public class KnifeWeaponController : WeaponBase, IWeaponUI
{
    protected override void Attack()
    {
        Vector2 dir = GetAimDirection();
        ExecuteFirePattern(dir);
    }

    // ================= TƯƠNG THÍCH NGƯỢC IWeaponUI =================
    public SpriteRenderer GetIcon()
    {
        return weaponData != null ? weaponData.weaponSprite : null;
    }

    public void UpdateWeaponData(int AddDame)
    {
        UpgradeWeapon();
    }
}

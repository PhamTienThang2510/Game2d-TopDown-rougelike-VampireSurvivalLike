using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] public WeaponData weaponData;
    [SerializeField] public int dame;
    [SerializeField] public float speed;
    [SerializeField] public float cooldown;
    [SerializeField] public Transform spawnHolder;

    public virtual void UpgradeWeapon()
    {
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string projectileKey;
    public float projectileSpeed = 10f;
    public SpriteRenderer weaponSprite;
    public int damage = 1;
    public float cooldown = 0.5f;
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Bộ quản lý kho vũ khí của người chơi (Weapon Inventory & Manager).
/// Quản lý danh sách vũ khí sở hữu, slot tối đa, thêm và nâng cấp vũ khí thông qua EventBus.
/// </summary>
public class WeaponController : MonoBehaviour
{
    private static WeaponController _instance;
    public static WeaponController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<WeaponController>();
                if (_instance == null)
                {
                    var holder = GameObject.Find("WeaponHolder");
                    if (holder != null)
                    {
                        _instance = holder.AddComponent<WeaponController>();
                    }
                    else
                    {
                        var player = GameObject.FindWithTag("Player");
                        if (player != null)
                        {
                            _instance = player.AddComponent<WeaponController>();
                        }
                        else
                        {
                            var go = new GameObject("WeaponController");
                            _instance = go.AddComponent<WeaponController>();
                        }
                    }
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Weapon Placement")]
    [SerializeField] private GameObject weaponHolder;

    [Header("Weapon Settings")]
    [SerializeField] private int maxWeapon = 3;

    [Header("All Weapons In Game")]
    [SerializeField] private List<GameObject> allWeapons = new();

    [Header("Currently Owned Weapons")]
    [SerializeField] private List<GameObject> currentWeapons = new();

    public int MaxWeapon => maxWeapon;
    public IReadOnlyList<GameObject> CurrentWeapons => currentWeapons;
    public IReadOnlyList<GameObject> AllWeapons => allWeapons;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        Initialize();
    }

    private void Initialize()
    {
        if (weaponHolder == null)
        {
            var found = GameObject.Find("WeaponHolder");
            if (found != null)
            {
                weaponHolder = found;
            }
            else
            {
                Transform holder = transform.Find("WeaponHolder");
                if (holder != null)
                    weaponHolder = holder.gameObject;
                else
                    weaponHolder = gameObject;
            }
        }

        // Tự động nạp danh sách vũ khí từ WeaponHolder nếu danh sách allWeapons chưa được gán
        if (allWeapons.Count == 0 && weaponHolder != null)
        {
            foreach (Transform child in weaponHolder.transform)
            {
                if (child.GetComponent<WeaponBase>() != null || child.GetComponent<IWeaponUI>() != null)
                {
                    allWeapons.Add(child.gameObject);
                }
            }
        }

        LoadCurrentWeapon();
    }

    private void OnEnable()
    {
        EventBus<OnWeaponSelectedEvent>.Subscribe(OnWeaponSelected);
    }

    private void OnDisable()
    {
        EventBus<OnWeaponSelectedEvent>.Unsubscribe(OnWeaponSelected);
    }

    // ================= EVENT LISTENER =================
    private void OnWeaponSelected(OnWeaponSelectedEvent evt)
    {
        if (evt.Weapon == null) return;

        if (HasWeapon(evt.Weapon))
        {
            UpgradeWeapon(evt.Weapon);
        }
        else
        {
            AddWeapon(evt.Weapon);
        }
    }

    // ================= LOAD CURRENT WEAPONS =================
    public void LoadCurrentWeapon()
    {
        currentWeapons.Clear();

        if (weaponHolder == null) return;

        foreach (Transform child in weaponHolder.transform)
        {
            if (child.gameObject.activeSelf)
            {
                currentWeapons.Add(child.gameObject);
            }
        }
    }

    // ================= WEAPON INVENTORY API =================
    public bool HasWeapon(GameObject weapon)
    {
        return currentWeapons.Contains(weapon);
    }

    public bool CanAddWeapon()
    {
        return currentWeapons.Count < maxWeapon;
    }

    public bool IsWeaponMaxLevel(GameObject weapon)
    {
        if (weapon == null) return true;
        var baseWeapon = weapon.GetComponent<WeaponBase>();
        return baseWeapon != null && baseWeapon.IsMaxLevel;
    }

    public void AddWeapon(GameObject weapon)
    {
        if (!CanAddWeapon()) return;

        if (weaponHolder != null && weapon.transform.parent != weaponHolder.transform)
        {
            weapon.transform.SetParent(weaponHolder.transform);
        }

        weapon.SetActive(true);

        if (!currentWeapons.Contains(weapon))
        {
            currentWeapons.Add(weapon);
        }
    }

    public void UpgradeWeapon(GameObject weapon)
    {
        var baseWeapon = weapon.GetComponent<WeaponBase>();
        if (baseWeapon != null)
        {
            baseWeapon.UpgradeWeapon();
            return;
        }

        var weaponUI = weapon.GetComponent<IWeaponUI>();
        if (weaponUI != null)
        {
            weaponUI.UpdateWeaponData(1);
        }
    }

    /// <summary>
    /// Trả về danh sách tất cả các vũ khí có thể xuất hiện trong bảng nâng cấp
    /// </summary>
    public List<GameObject> GetAvailableWeaponUpgrades()
    {
        if (weaponHolder == null || allWeapons.Count == 0)
        {
            Initialize();
        }

        LoadCurrentWeapon();

        List<GameObject> upgradeableCurrent = currentWeapons
            .Where(w => !IsWeaponMaxLevel(w))
            .ToList();

        if (CanAddWeapon())
        {
            List<GameObject> unownedWeapons = allWeapons
                .Where(w => !HasWeapon(w) && !IsWeaponMaxLevel(w))
                .ToList();

            return unownedWeapons.Concat(upgradeableCurrent).Distinct().ToList();
        }

        return upgradeableCurrent;
    }
}

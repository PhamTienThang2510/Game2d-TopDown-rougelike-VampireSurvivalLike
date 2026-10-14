using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerExp playerExp;
    [SerializeField] private GameObject weaponHolder;

    [Header("All Weapons")]
    [SerializeField] private List<GameObject> allWeapons = new();

    [Header("All Passives")]
    [SerializeField] private List<PassiveItemData> allPassives = new();

    [Header("Weapon Settings")]
    [SerializeField] private int maxWeapon = 3;

    [Header("Health UI")]
    [SerializeField] private Slider sliderHealth;
    [SerializeField] private TextMeshProUGUI textHealthSlider;

    [Header("Exp UI")]
    [SerializeField] private Slider sliderExp;
    [SerializeField] private TextMeshProUGUI textExpSlider;

    [Header("Panels")]
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelUpgrade;
    [SerializeField] private GameObject panelPause;
    [Header("Upgrade Option UI")]

    [SerializeField]private List<GameObject> currentWeapons = new();
    private bool isPaused;

    private void Awake()
    {
        instance = this;
        panelGameOver.SetActive(false);
        panelUpgrade.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus<OnGameOverEvent>.Subscribe(OnGameOver);
        EventBus<OnLevelUpEvent>.Subscribe(OnLevelUp);
        EventBus<OnHealthChangedEvent>.Subscribe(OnHealthChanged);
        EventBus<OnExpChangedEvent>.Subscribe(OnExpChanged);
    }

    private void OnDisable()
    {
        EventBus<OnGameOverEvent>.Unsubscribe(OnGameOver);
        EventBus<OnLevelUpEvent>.Unsubscribe(OnLevelUp);
        EventBus<OnHealthChangedEvent>.Unsubscribe(OnHealthChanged);
        EventBus<OnExpChangedEvent>.Unsubscribe(OnExpChanged);
    }

    private void Start()
    {
        LoadCurrentWeapon();

        // Nếu bắt đầu mà chưa có vũ khí nào, mở giao diện pick (tạm dừng game) để người chơi chọn vũ khí khởi đầu
        if (currentWeapons.Count == 0)
        {
            Upgrade();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    public void TogglePause()
    {
        // Không cho pause khi game over hoặc đang upgrade
        if (panelGameOver.activeSelf || panelUpgrade.activeSelf)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        panelPause.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        panelPause.SetActive(false);
    }

    // ================= HEALTH =================
    private void OnHealthChanged(OnHealthChangedEvent evt)
    {
        SettingHealthUI(evt.CurrentHealth, evt.MaxHealth);
    }

    public void SettingHealthUI(int current, int max)
    {
        sliderHealth.value = current * 1f / max;
        textHealthSlider.text = $"{current}/{max}";
    }

    // ================= EXP =================
    private void OnExpChanged(OnExpChangedEvent evt)
    {
        SettingExpUI(evt.CurrentExp, evt.ExpToLevelUp);
    }

    public void SettingExpUI(int current, int max)
    {
        sliderExp.value = current * 1f / max;
        textExpSlider.text = $"{current}/{max}";
    }

    // ================= GAME OVER =================
    private void OnGameOver(OnGameOverEvent _)
    {
        ShowGameOverPanel();
    }

    private void OnLevelUp(OnLevelUpEvent _)
    {
        Upgrade();
    }

    private void ShowGameOverPanel()
    {
        panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    // ================= LOAD CURRENT WEAPON =================
    private void LoadCurrentWeapon()
    {
        currentWeapons.Clear();

        foreach (Transform weapon in weaponHolder.transform)
        {
            if (weapon.gameObject.activeSelf)
                currentWeapons.Add(weapon.gameObject);
        }
    }

    // ================= UPGRADE =================
    private struct UpgradeOptionCandidate
    {
        public bool isWeapon;
        public GameObject weapon;
        public PassiveItemData passive;

        public static UpgradeOptionCandidate Weapon(GameObject w) => new() { isWeapon = true, weapon = w };
        public static UpgradeOptionCandidate Passive(PassiveItemData p) => new() { isWeapon = false, passive = p };
    }

    private void Upgrade()
    {
        Time.timeScale = 0f;
        panelUpgrade.SetActive(true);

        LoadCurrentWeapon();

        List<UpgradeOptionCandidate> candidatePool = new();

        // 1️⃣ THÊM VŨ KHÍ HỢP LỆ VÀO POOL
        List<GameObject> upgradeableCurrentWeapons = currentWeapons
            .Where(w => !IsWeaponMaxLevel(w))
            .ToList();

        if (currentWeapons.Count < maxWeapon)
        {
            List<GameObject> unownedWeapons = allWeapons
                .Where(w => !HasWeapon(w) && !IsWeaponMaxLevel(w))
                .ToList();

            foreach (var w in unownedWeapons) candidatePool.Add(UpgradeOptionCandidate.Weapon(w));
            foreach (var w in upgradeableCurrentWeapons) candidatePool.Add(UpgradeOptionCandidate.Weapon(w));
        }
        else
        {
            foreach (var w in upgradeableCurrentWeapons) candidatePool.Add(UpgradeOptionCandidate.Weapon(w));
        }

        // 2️⃣ THÊM PASSIVE ITEM HỢP LỆ VÀO POOL (Chỉ xuất hiện khi người chơi đã có ít nhất 1 vũ khí khởi đầu)
        bool hasStartingWeapon = currentWeapons.Count > 0;

        if (hasStartingWeapon && PlayerPassiveManager.Instance != null && allPassives != null && allPassives.Count > 0)
        {
            bool canAddPassive = PlayerPassiveManager.Instance.CanAddPassive();

            foreach (var passive in allPassives)
            {
                if (passive == null) continue;
                bool isOwned = PlayerPassiveManager.Instance.HasPassive(passive);
                bool isMax = PlayerPassiveManager.Instance.IsMaxLevel(passive);

                if (isMax) continue;

                if (isOwned || canAddPassive)
                {
                    candidatePool.Add(UpgradeOptionCandidate.Passive(passive));
                }
            }
        }

        // 3️⃣ NẾU KHÔNG CÒN GÌ ĐỂ NÂNG CẤP
        if (candidatePool.Count == 0)
        {
            panelUpgrade.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        // 4️⃣ RANDOM VÀ LOẠI TRÙNG
        var selected = candidatePool
            .Distinct()
            .OrderBy(_ => Random.value)
            .ToList();

        // 5️⃣ GÁN LÊN UI
        var optionUIs = panelUpgrade.GetComponentsInChildren<UpgradeOptionUI>(true);

        for (int i = 0; i < optionUIs.Length; i++)
        {
            if (i < selected.Count)
            {
                optionUIs[i].gameObject.SetActive(true);
                if (selected[i].isWeapon)
                    optionUIs[i].SetupWeapon(selected[i].weapon);
                else
                    optionUIs[i].SetupPassive(selected[i].passive);
            }
            else
            {
                optionUIs[i].gameObject.SetActive(false);
            }
        }
    }

    private bool IsWeaponMaxLevel(GameObject weapon)
    {
        if (weapon == null) return true;
        var ctrl = weapon.GetComponent<WeaponController>();
        return ctrl != null && ctrl.IsMaxLevel;
    }

    // ================= WEAPON & PASSIVE API =================
    public bool HasWeapon(GameObject weapon)
    {
        return currentWeapons.Contains(weapon);
    }

    public bool CanAddWeapon()
    {
        return currentWeapons.Count < maxWeapon;
    }

    public void AddWeapon(GameObject weapon)
    {
        if (!CanAddWeapon()) return;

        weapon.transform.SetParent(weaponHolder.transform);
        weapon.SetActive(true);

        currentWeapons.Add(weapon);
    }

    public void PickWeapon(GameObject weapon)
    {
        if (HasWeapon(weapon))
            UpgradeWeapon(weapon);
        else
            AddWeapon(weapon);

        panelUpgrade.SetActive(false);
        Time.timeScale = 1f;
    }

    public void PickPassive(PassiveItemData passive)
    {
        if (PlayerPassiveManager.Instance != null && passive != null)
        {
            PlayerPassiveManager.Instance.AddOrUpgradePassive(passive);
        }

        panelUpgrade.SetActive(false);
        Time.timeScale = 1f;
    }

    public void UpgradeWeapon(GameObject weapon)
    {
        var ctrl = weapon.GetComponent<WeaponController>();
        if (ctrl != null)
        {
            ctrl.UpgradeWeapon();
            return;
        }

        var weaponUI = weapon.GetComponent<IWeaponUI>();
        if (weaponUI != null)
        {
            weaponUI.UpdateWeaponData(1);
        }
    }
}

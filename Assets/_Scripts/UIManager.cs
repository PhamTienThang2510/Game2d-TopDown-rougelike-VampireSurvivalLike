using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý giao diện người dùng (HUD, Pause, Game Over, Bảng nâng cấp khi lên cấp).
/// Tách biệt hoàn toàn khỏi logic quản lý kho vũ khí (đã chuyển sang WeaponController).
/// Giao tiếp hoàn toàn thông qua EventBus.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerExp playerExp;

    [Header("All Passives")]
    [SerializeField] private List<PassiveItemData> allPassives = new();

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

    private bool isPaused;

    private void Awake()
    {
        instance = this;
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
        if (panelPause != null) panelPause.SetActive(false);
    }

    private void OnEnable()
    {
        EventBus<OnGameOverEvent>.Subscribe(OnGameOver);
        EventBus<OnLevelUpEvent>.Subscribe(OnLevelUp);
        EventBus<OnHealthChangedEvent>.Subscribe(OnHealthChanged);
        EventBus<OnExpChangedEvent>.Subscribe(OnExpChanged);
        EventBus<OnUpgradeCompletedEvent>.Subscribe(OnUpgradeCompleted);
    }

    private void OnDisable()
    {
        EventBus<OnGameOverEvent>.Unsubscribe(OnGameOver);
        EventBus<OnLevelUpEvent>.Unsubscribe(OnLevelUp);
        EventBus<OnHealthChangedEvent>.Unsubscribe(OnHealthChanged);
        EventBus<OnExpChangedEvent>.Unsubscribe(OnExpChanged);
        EventBus<OnUpgradeCompletedEvent>.Unsubscribe(OnUpgradeCompleted);
    }

    private void Start()
    {
        // Nếu người chơi chưa có vũ khí khởi đầu nào, mở bảng chọn vũ khí
        if (WeaponController.Instance != null && WeaponController.Instance.CurrentWeapons.Count == 0)
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
        // Không cho pause khi game over hoặc đang trong bảng nâng cấp
        if ((panelGameOver != null && panelGameOver.activeSelf) || (panelUpgrade != null && panelUpgrade.activeSelf))
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
        if (panelPause != null) panelPause.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (panelPause != null) panelPause.SetActive(false);
    }

    // ================= HEALTH =================
    private void OnHealthChanged(OnHealthChangedEvent evt)
    {
        SettingHealthUI(evt.CurrentHealth, evt.MaxHealth);
    }

    public void SettingHealthUI(int current, int max)
    {
        if (sliderHealth != null)
            sliderHealth.value = current * 1f / max;
        if (textHealthSlider != null)
            textHealthSlider.text = $"{current}/{max}";
    }

    // ================= EXP =================
    private void OnExpChanged(OnExpChangedEvent evt)
    {
        SettingExpUI(evt.CurrentExp, evt.ExpToLevelUp);
    }

    public void SettingExpUI(int current, int max)
    {
        if (sliderExp != null)
            sliderExp.value = current * 1f / max;
        if (textExpSlider != null)
            textExpSlider.text = $"{current}/{max}";
    }

    // ================= GAME OVER =================
    private void OnGameOver(OnGameOverEvent _)
    {
        ShowGameOverPanel();
    }

    private void ShowGameOverPanel()
    {
        if (panelGameOver != null) panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    // ================= UPGRADE FLOW =================
    private struct UpgradeOptionCandidate
    {
        public bool isWeapon;
        public GameObject weapon;
        public PassiveItemData passive;

        public static UpgradeOptionCandidate Weapon(GameObject w) => new() { isWeapon = true, weapon = w };
        public static UpgradeOptionCandidate Passive(PassiveItemData p) => new() { isWeapon = false, passive = p };
    }

    private void OnLevelUp(OnLevelUpEvent _)
    {
        Upgrade();
    }

    private void OnUpgradeCompleted(OnUpgradeCompletedEvent _)
    {
        if (panelUpgrade != null)
            panelUpgrade.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Upgrade()
    {
        if (panelUpgrade == null) return;

        Time.timeScale = 0f;
        panelUpgrade.SetActive(true);

        List<UpgradeOptionCandidate> candidatePool = new();

        // 1️⃣ Lấy danh sách nâng cấp Vũ khí từ WeaponController
        if (WeaponController.Instance != null)
        {
            var availableWeapons = WeaponController.Instance.GetAvailableWeaponUpgrades();
            if (availableWeapons != null)
            {
                foreach (var w in availableWeapons)
                {
                    if (w != null)
                        candidatePool.Add(UpgradeOptionCandidate.Weapon(w));
                }
            }
        }

        // 2️⃣ Lấy danh sách nâng cấp Passive Item từ PlayerPassiveManager
        // (Chỉ xuất hiện khi người chơi đã có ít nhất 1 vũ khí khởi đầu)
        bool hasStartingWeapon = WeaponController.Instance != null && WeaponController.Instance.CurrentWeapons.Count > 0;
        if (hasStartingWeapon && PlayerPassiveManager.Instance != null)
        {
            var availablePassives = PlayerPassiveManager.Instance.GetAvailablePassiveUpgrades(allPassives);
            if (availablePassives != null)
            {
                foreach (var p in availablePassives)
                {
                    if (p != null)
                        candidatePool.Add(UpgradeOptionCandidate.Passive(p));
                }
            }
        }

        // 3️⃣ Nếu không còn bất kỳ lựa chọn nào để nâng cấp -> Tự động bỏ qua và tiếp tục game
        if (candidatePool.Count == 0)
        {
            panelUpgrade.SetActive(false);
            Time.timeScale = 1f;
            return;
        }

        // 4️⃣ Random xáo trộn và loại trùng
        var selected = candidatePool
            .Distinct()
            .OrderBy(_ => Random.value)
            .ToList();

        // 5️⃣ Gán lên các ô UpgradeOptionUI
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
}

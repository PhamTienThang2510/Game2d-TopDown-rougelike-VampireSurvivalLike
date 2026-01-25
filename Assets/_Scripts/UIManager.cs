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

    private void Start()
    {
        player.OnStateGameOver += ShowGameOverPanel;
        playerExp.OnStateGameUpgrade += Upgrade;
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
    public void SettingHealthUI(int current, int max)
    {
        sliderHealth.value = current * 1f / max;
        textHealthSlider.text = $"{current}/{max}";
    }

    // ================= EXP =================
    public void SettingExpUI(int current, int max)
    {
        sliderExp.value = current * 1f / max;
        textExpSlider.text = $"{current}/{max}";
    }

    // ================= GAME OVER =================
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
    private void Upgrade()
    {
        Time.timeScale = 0f;
        panelUpgrade.SetActive(true);

        LoadCurrentWeapon();

        // 1️⃣ TẠO POOL
        List<GameObject> upgradePool = new();

        if (currentWeapons.Count < maxWeapon)
            upgradePool.AddRange(allWeapons);
        else
            upgradePool.AddRange(currentWeapons);

        // 2️⃣ RANDOM
        var candidates = upgradePool
            .OrderBy(_ => Random.value)
            .ToList();

        // 3️⃣ LẤY OPTION UI TỪ PANEL
        var optionUIs = panelUpgrade
            .GetComponentsInChildren<UpgradeOptionUI>(true);

        for (int i = 0; i < optionUIs.Length; i++)
        {
            if (i < candidates.Count)
            {
                optionUIs[i].gameObject.SetActive(true);
                optionUIs[i].Setup(candidates[i]);
            }
            else
            {
                optionUIs[i].gameObject.SetActive(false);
            }
        }
    }




    // ================= WEAPON API =================
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

    public void UpgradeWeapon(GameObject weapon)
    {
        var weaponUI = weapon.GetComponent<IWeaponUI>();
        if (weaponUI != null)
        {
            weaponUI.UpdateWeaponData(1);
        }
    }
}

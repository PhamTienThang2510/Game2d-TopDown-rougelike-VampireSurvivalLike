using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image weaponImage;
    [SerializeField] private Button pickButton;
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textLevel;
    [SerializeField] private TextMeshProUGUI textDescription;

    private GameObject weapon;
    private PassiveItemData passiveData;

    /// <summary>
    /// Cấu hình cho lựa chọn là Vũ Khí
    /// </summary>
    public void SetupWeapon(GameObject weaponObj)
    {
        this.weapon = weaponObj;
        this.passiveData = null;

        var weaponBase = weaponObj.GetComponent<WeaponBase>();
        var weaponUI = weaponObj.GetComponent<IWeaponUI>();

        // 1. Icon
        if (weaponImage != null)
        {
            if (weaponBase != null && weaponBase.GetIconSprite() != null)
                weaponImage.sprite = weaponBase.GetIconSprite();
            else if (weaponUI != null && weaponUI.GetIcon() != null)
                weaponImage.sprite = weaponUI.GetIcon().sprite;
        }

        // 2. Name & Description & Level
        if (weaponBase != null)
        {
            if (textName != null)
                textName.text = weaponBase.GetWeaponName();

            bool isOwned = WeaponController.Instance != null && WeaponController.Instance.HasWeapon(weaponObj);

            if (textLevel != null)
            {
                textLevel.text = isOwned
                    ? $"Cấp {weaponBase.currentLevel} ➔ {weaponBase.currentLevel + 1}"
                    : "MỚI!";
            }

            if (textDescription != null)
            {
                textDescription.text = isOwned
                    ? weaponBase.GetNextUpgradeDescription()
                    : (weaponBase.weaponData != null ? weaponBase.weaponData.baseDescription : string.Empty);
            }
        }

        SetupButton();
    }

    /// <summary>
    /// Cấu hình cho lựa chọn là Trang Bị Phụ Trợ (Passive Item)
    /// </summary>
    public void SetupPassive(PassiveItemData passive)
    {
        this.passiveData = passive;
        this.weapon = null;

        if (passive == null) return;

        // 1. Icon
        if (weaponImage != null)
        {
            weaponImage.sprite = passive.icon;
        }

        // 2. Name
        if (textName != null)
        {
            textName.text = passive.passiveName;
        }

        // 3. Level & Description
        int currentLvl = (PlayerPassiveManager.Instance != null)
            ? PlayerPassiveManager.Instance.GetPassiveLevel(passive)
            : 0;

        if (textLevel != null)
        {
            textLevel.text = (currentLvl == 0)
                ? "MỚI!"
                : $"Cấp {currentLvl} ➔ {currentLvl + 1}";
        }

        if (textDescription != null)
        {
            textDescription.text = (currentLvl == 0)
                ? passive.baseDescription
                : passive.GetUpgradeDescription(currentLvl + 1);
        }

        SetupButton();
    }

    /// <summary>
    /// Tương thích ngược với các lời gọi cũ
    /// </summary>
    public void Setup(GameObject weaponObj)
    {
        SetupWeapon(weaponObj);
    }

    private void SetupButton()
    {
        if (pickButton != null)
        {
            pickButton.onClick.RemoveAllListeners();
            pickButton.onClick.AddListener(Pick);
        }
    }

    private void Pick()
    {
        if (weapon != null)
        {
            EventBus<OnWeaponSelectedEvent>.Publish(new OnWeaponSelectedEvent(weapon));
            EventBus<OnUpgradeCompletedEvent>.Publish();
        }
        else if (passiveData != null)
        {
            EventBus<OnPassiveSelectedEvent>.Publish(new OnPassiveSelectedEvent(passiveData));
            EventBus<OnUpgradeCompletedEvent>.Publish();
        }
    }
}

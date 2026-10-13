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

    public void Setup(GameObject weapon)
    {
        this.weapon = weapon;

        var controller = weapon.GetComponent<WeaponController>();
        var weaponUI = weapon.GetComponent<IWeaponUI>();

        // 1. Icon
        if (weaponImage != null)
        {
            if (controller != null && controller.GetIconSprite() != null)
                weaponImage.sprite = controller.GetIconSprite();
            else if (weaponUI != null && weaponUI.GetIcon() != null)
                weaponImage.sprite = weaponUI.GetIcon().sprite;
        }

        // 2. Name & Description & Level
        if (controller != null)
        {
            if (textName != null)
                textName.text = controller.GetWeaponName();

            bool isOwned = UIManager.instance != null && UIManager.instance.HasWeapon(weapon);

            if (textLevel != null)
            {
                textLevel.text = isOwned
                    ? $"Cấp {controller.currentLevel} ➔ {controller.currentLevel + 1}"
                    : "MỚI!";
            }

            if (textDescription != null)
            {
                textDescription.text = isOwned
                    ? controller.GetNextUpgradeDescription()
                    : (controller.weaponData != null ? controller.weaponData.baseDescription : string.Empty);
            }
        }

        // 3. Button
        if (pickButton != null)
        {
            pickButton.onClick.RemoveAllListeners();
            pickButton.onClick.AddListener(Pick);
        }
    }

    private void Pick()
    {
        if (UIManager.instance != null && weapon != null)
        {
            UIManager.instance.PickWeapon(weapon);
        }
    }
}

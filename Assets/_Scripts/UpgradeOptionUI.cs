using UnityEngine;
using UnityEngine.UI;

public class UpgradeOptionUI : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private Button pickButton;

    private GameObject weapon;

    public void Setup(GameObject weapon)
    {
        this.weapon = weapon;

        var weaponUI = weapon.GetComponent<IWeaponUI>();
        if (weaponUI != null)
            weaponImage.sprite = weaponUI.GetIcon().sprite;

        pickButton.onClick.RemoveAllListeners();
        pickButton.onClick.AddListener(Pick);
    }

    private void Pick()
    {
        UIManager.instance.PickWeapon(weapon);
    }
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Thông tin hiển thị")]
    public string weaponName = "New Weapon";
    public Sprite icon;
    [TextArea(2, 4)]
    public string baseDescription = "Mô tả vũ khí";

    [Header("Chỉ số cơ bản (Tương thích ngược các .asset cũ)")]
    public string projectileKey;
    public float projectileSpeed = 10f;
    public SpriteRenderer weaponSprite;
    public int damage = 1;
    public float cooldown = 0.5f;

    [Header("Cấu hình Cấp độ (Level 1 -> Max)")]
    public List<WeaponLevelData> levels = new();

    [Header("Trạng thái Tiến Hóa (Weapon Evolution)")]
    [Tooltip("Đánh dấu nếu vũ khí này là dạng đã tiến hóa (Legendary / Ultimate)")]
    public bool isEvolution;

    [Tooltip("ScriptableObject công thức tiến hóa (nếu vũ khí này có thể tiến hóa)")]
    public WeaponEvolutionData evolutionData;

    /// <summary>
    /// Số cấp độ tối đa của vũ khí này
    /// </summary>
    public int MaxLevel => (levels != null && levels.Count > 0) ? levels.Count : 8;

    /// <summary>
    /// Lấy Icon (ưu tiên Sprite icon, nếu null fallback sang weaponSprite cũ)
    /// </summary>
    public Sprite GetIcon()
    {
        if (icon != null) return icon;
        if (weaponSprite != null) return weaponSprite.sprite;
        return null;
    }

    /// <summary>
    /// Lấy dữ liệu của cấp độ cụ thể (level bắt đầu từ 1).
    /// Tự động fallback thông minh nếu danh sách levels chưa được cấu hình trong Editor.
    /// </summary>
    public WeaponLevelData GetLevelData(int level)
    {
        if (levels != null && levels.Count > 0)
        {
            int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
            return levels[index];
        }

        // Fallback tự động dựa trên các chỉ số cơ bản
        int projCount = 1 + (level >= 3 ? 1 : 0) + (level >= 6 ? 1 : 0);
        return new WeaponLevelData
        {
            damage = this.damage + Mathf.Max(0, level - 1),
            cooldown = Mathf.Max(0.1f, this.cooldown - (level - 1) * 0.05f),
            projectileSpeed = this.projectileSpeed,
            projectileCount = projCount,
            areaScale = 1f + (level - 1) * 0.1f,
            duration = 1f,
            firePattern = (projCount > 1) ? FirePattern.FanSpread : FirePattern.Forward,
            motionType = ProjectileMotionType.Straight,
            spreadAngle = 15f,
            spinSpeed = 720f,
            waveFrequency = 10f,
            waveAmplitude = 1.2f,
            upgradeDescription = $"+1 Sát thương (Cấp {level})"
        };
    }

    /// <summary>
    /// Lấy câu mô tả khi lên cấp tiếp theo
    /// </summary>
    public string GetUpgradeDescription(int level)
    {
        WeaponLevelData lvlData = GetLevelData(level);
        return lvlData != null ? lvlData.upgradeDescription : string.Empty;
    }
}

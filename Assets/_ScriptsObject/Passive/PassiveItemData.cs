using System;
using System.Collections.Generic;
using UnityEngine;

public enum PassiveStatType
{
    Attack,             // Tăng % Sát thương toàn bộ vũ khí
    Cooldown,           // Giảm % Thời gian hồi chiêu
    MoveSpeed,          // Tăng % Tốc độ di chuyển của nhân vật
    Area,               // Tăng % Kích thước đạn / Vùng ảnh hưởng
    BulletSpeed,        // Tăng % Tốc độ bay của đạn
    AmountBullet,       // Tăng số lượng đạn bắn thêm (+1, +2...)
    MagnetRange,        // Tăng % Bán kính nhặt ngọc EXP
    MaxHealth,          // Tăng máu tối đa của nhân vật
    Recovery,           // Hồi máu theo thời gian (HP / giây)
    Armor               // Giảm sát thương nhận vào
}

[Serializable]
public class PassiveLevelData
{
    [Tooltip("Giá trị cộng thêm ở cấp này (Ví dụ: 0.1 cho +10%, hoặc 1 cho +1 đạn)")]
    public float Value;

    [Tooltip("Câu mô tả hiển thị trên bảng nâng cấp (Ví dụ: '+10% Sát thương toàn bộ vũ khí')")]
    public string upgradeDescription;
}

[CreateAssetMenu(fileName = "PassiveItemData", menuName = "Scriptable Objects/PassiveItemData")]
public class PassiveItemData : ScriptableObject
{
    [Header("Thông tin hiển thị")]
    public string passiveName = "New Passive";
    public Sprite icon;
    [TextArea(2, 4)]
    public string baseDescription = "Mô tả trang bị phụ trợ.";

    [Header("Loại chỉ số")]
    public PassiveStatType statType = PassiveStatType.Attack;

    [Header("Danh sách Cấp độ (Level 1 -> Max)")]
    public List<PassiveLevelData> levels = new();

    public int MaxLevel => levels != null ? levels.Count : 0;

    /// <summary>
    /// Lấy dữ liệu cấp độ cụ thể (level bắt đầu từ 1)
    /// </summary>
    public PassiveLevelData GetLevelData(int level)
    {
        if (levels == null || levels.Count == 0) return null;
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }

    /// <summary>
    /// Lấy câu mô tả cho cấp độ tiếp theo khi nâng cấp
    /// </summary>
    public string GetUpgradeDescription(int level)
    {
        PassiveLevelData data = GetLevelData(level);
        return data != null ? data.upgradeDescription : string.Empty;
    }
}

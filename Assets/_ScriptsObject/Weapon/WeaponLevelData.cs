using System;
using UnityEngine;

/// <summary>
/// Chứa toàn bộ thông số và mô tả của một cấp độ vũ khí cụ thể.
/// </summary>
[Serializable]
public class WeaponLevelData
{
    [Header("Chỉ số ở cấp độ này")]
    [Tooltip("Sát thương gây ra trên mỗi đòn đánh/viên đạn")]
    public int damage = 1;

    [Tooltip("Thời gian hồi chiêu giữa mỗi đợt tấn công (giây)")]
    public float cooldown = 1f;

    [Tooltip("Tốc độ bay của đạn")]
    public float projectileSpeed = 10f;

    [Tooltip("Số lượng đạn / tia sét / rìu / lần đánh mỗi đợt")]
    public int projectileCount = 1;

    [Tooltip("Tỉ lệ kích thước đạn hoặc diện tích quét (1.0 = 100%)")]
    public float areaScale = 1f;

    [Tooltip("Thời gian tồn tại của chiêu thức (khiên/đạn)")]
    public float duration = 1f;

    [Header("Cấu hình Tấn Công & Quỹ Đạo")]
    [Tooltip("Kiểu xuất chiêu của vũ khí (Bắn thẳng, rẻ quạt, tỏa tròn, v.v.)")]
    public FirePattern firePattern = FirePattern.Forward;

    [Tooltip("Kiểu chuyển động của đạn khi bay (Bay thẳng, tự xoay, lượn sóng Sin, v.v.)")]
    public ProjectileMotionType motionType = ProjectileMotionType.Straight;

    [Tooltip("Góc lệch giữa các viên đạn khi bắn rẻ quạt (độ)")]
    public float spreadAngle = 15f;

    [Tooltip("Tốc độ tự xoay nếu chọn Spinning (độ/giây)")]
    public float spinSpeed = 720f;

    [Tooltip("Tần số lượn sóng nếu chọn SineWave")]
    public float waveFrequency = 10f;

    [Tooltip("Biên độ sóng nếu chọn SineWave")]
    public float waveAmplitude = 1.2f;

    [Header("Mô tả nâng cấp")]
    [Tooltip("Câu mô tả hiển thị trên UI khi người chơi nhìn thấy lựa chọn lên cấp này")]
    public string upgradeDescription = "+10% Sát thương";
}

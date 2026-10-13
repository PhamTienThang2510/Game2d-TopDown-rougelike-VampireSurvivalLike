/// <summary>
/// Kiểu xuất chiêu / phóng đạn của vũ khí
/// </summary>
public enum FirePattern
{
    Forward,        // Bắn thẳng theo hướng nhìn / di chuyển của người chơi
    FanSpread,      // Bắn rẻ quạt tỏa góc (vd: -15, 0, +15 độ)
    Radial360,      // Phóng đạn tỏa đều 360 độ quanh người chơi
    Burst,          // Bắn liên thanh N phát liên tiếp
    TargetEnemy     // Nhắm bắn thẳng vào kẻ địch gần nhất
}

/// <summary>
/// Kiểu quỹ đạo / chuyển động của viên đạn khi bay
/// </summary>
public enum ProjectileMotionType
{
    Straight,       // Bay thẳng tắp theo hướng bắn
    Spinning,       // Vừa bay vừa tự xoay tít quanh tâm (như phi tiêu xoay)
    SineWave,       // Bay theo quỹ đạo lượn sóng Sin (chéo qua lại)
    Boomerang,      // Bay ra một khoảng rồi lượn quay trở về người chơi
    Homing          // Tự động rượt đuổi mục tiêu
}

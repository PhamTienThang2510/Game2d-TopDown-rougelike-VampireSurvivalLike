using UnityEngine;

/// <summary>
/// ScriptableObject quản lý công thức và trạng thái tiến hóa vũ khí (Weapon Evolution).
/// Ví dụ: Dao găm (Cấp tối đa) + Găng tay (Passive) => Thousand Edge (Tiến hóa).
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponEvolution", menuName = "Scriptable Objects/WeaponEvolutionData")]
public class WeaponEvolutionData : ScriptableObject
{
    [Header("1. Điều kiện Tiến hóa")]
    [Tooltip("Vũ khí cơ bản cần nâng cấp")]
    public WeaponData baseWeapon;

    [Tooltip("Trang bị phụ trợ (Passive) bắt buộc người chơi phải sở hữu để kích hoạt tiến hóa")]
    public PassiveItemData requiredPassive;

    [Tooltip("Cấp độ vũ khí cơ bản tối thiểu để có thể tiến hóa (thường là cấp tối đa, ví dụ 8)")]
    public int requiredWeaponLevel = 8;

    [Tooltip("Có yêu cầu Passive cũng phải đạt cấp tối đa hay chỉ cần sở hữu ít nhất cấp 1?")]
    public bool requirePassiveMaxLevel = false;

    [Header("2. Kết quả Tiến hóa")]
    [Tooltip("ScriptableObject của Vũ khí sau khi tiến hóa")]
    public WeaponData evolutionWeapon;

    [Tooltip("Prefab của Vũ khí sau khi tiến hóa (nếu vũ khí mới là GameObject độc lập được thay thế)")]
    public GameObject evolutionWeaponPrefab;

    [Header("3. Thông tin hiển thị")]
    [Tooltip("Tên của vũ khí tiến hóa")]
    public string evolutionName = "Evolved Weapon";

    [TextArea(2, 4)]
    [Tooltip("Mô tả sức mạnh đặc biệt khi tiến hóa")]
    public string evolutionDescription = "Vũ khí đạt ngưỡng sức mạnh tối thượng!";

    /// <summary>
    /// Kiểm tra xem đã đủ điều kiện để kích hoạt tiến hóa vũ khí hay chưa
    /// </summary>
    /// <param name="currentWeaponLevel">Cấp độ hiện tại của vũ khí cơ bản</param>
    /// <param name="currentPassiveLevel">Cấp độ của passive tương ứng (0 nếu chưa có)</param>
    public bool CanEvolution(int currentWeaponLevel, int currentPassiveLevel)
    {
        if (currentWeaponLevel < requiredWeaponLevel)
            return false;

        if (requirePassiveMaxLevel)
        {
            int maxPassive = (requiredPassive != null) ? requiredPassive.MaxLevel : 1;
            return currentPassiveLevel >= maxPassive;
        }

        return currentPassiveLevel >= 1;
    }
}

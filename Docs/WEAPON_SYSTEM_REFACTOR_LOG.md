# 📖 NHẬT KÝ PHÁT TRIỂN & TÀI LIỆU CẢI TIẾN HỆ THỐNG VŨ KHÍ & NÂNG CẤP
**Ngày thực hiện:** 13/10/2026  
**Dự án:** 2D Top-Down Roguelike (Vampire Survivors-like)  
**Mục tiêu chính:** Chuyển đổi hệ thống vũ khí từ cơ chế tăng 1 sát thương đơn điệu sang hệ thống đa cấp độ (Level 1 -> 8), quỹ đạo bắn mô-đun hóa linh hoạt, chuẩn bị sẵn kiến trúc cho Đồ nội tại (Passive Items) và Tiến hóa vũ khí (Weapon Evolution).

---

## 🎯 1. Bối cảnh & Vấn đề Trước Khi Cải Tiến

| Thành phần | Trước khi cải tiến | Hạn chế thực tế |
| :--- | :--- | :--- |
| **Cấp độ vũ khí** | Chỉ gọi `UpdateWeaponData(1)` (tăng 1 sát thương) | Không có khái niệm cấp độ (Level 1 -> 8), không có Max Level, vũ khí nâng cấp vô hạn không có điểm dừng. |
| **Giao diện nâng cấp (UI)** | Chỉ hiển thị icon vũ khí và nút Pick | Người chơi không biết vũ khí tên gì, đang ở cấp mấy, nâng cấp mang lại lợi ích gì (+đạn, +vùng quét, giảm hồi chiêu). |
| **Vũ khí đạt Max Level** | Vẫn tiếp tục xuất hiện trong bảng quay thưởng | Lãng phí slot nâng cấp của người chơi khi lên cấp. |
| **Quỹ đạo bay & Kiểu bắn** | Hardcode cố định trong từng class vũ khí | Không thể đổi kiểu bay (bay thẳng, xoay tít, lượn sóng) nếu không phải sửa code C#. |
| **Code lặp lại (Boilerplate)** | Cả 6 file vũ khí đều tự đếm `timer`, tự reset `cooldown`, tự viết `Update()` | Vi phạm nguyên tắc DRY, khó bảo trì, tốn công khi tạo vũ khí mới. |
| **Khả năng mở rộng tương lai** | Chưa có kiến trúc cho Passive Items và Tiến hóa | Nếu thêm Passive sau này sẽ phải đập đi xây lại code từng vũ khí. |

---

## 🛠️ 2. Danh Sách Các File Đã Tạo Mới & Chỉnh Sửa

### 📁 A. Tầng ScriptableObject (Dữ Liệu)
1. **`Assets/_ScriptsObject/Passive/PassiveItemData.cs`** *(Tạo mới)*:
   - **Nội dung:** Định nghĩa ScriptableObject cho trang bị phụ trợ (Passive Items) với enum `PassiveStatType` (Attack, Cooldown, MoveSpeed, Area, BulletSpeed, AmountBullet, MagnetRange, MaxHealth, Recovery, Armor) và danh sách cấp độ `PassiveLevelData`.
   - **Lý do:** Chuẩn bị sẵn dữ liệu nền tảng cho Giai đoạn 2 để người chơi có thể trang bị các vật phẩm tăng chỉ số toàn cục (như Găng tay, Sách phép, Nam châm).
2. **`Assets/_ScriptsObject/Weapon/WeaponEnums.cs`** *(Tạo mới)*:
   - **Nội dung:** Định nghĩa 2 bộ Enum:
     - `FirePattern`: `Forward` (thẳng), `FanSpread` (rẻ quạt), `Radial360` (tỏa tròn), `Burst` (liên thanh), `TargetEnemy` (nhắm quái).
     - `ProjectileMotionType`: `Straight` (bay thẳng), `Spinning` (tự xoay tròn), `SineWave` (lượn sóng Sin), `Boomerang` (bay ra rồi quay lại), `Homing` (đuổi theo mục tiêu).
   - **Lý do:** Tách biệt kiểu xuất chiêu và quỹ đạo bay thành các module độc lập, cho phép đổi hành vi của đạn ngay trên Unity Inspector mà không cần viết code mới.
3. **`Assets/_ScriptsObject/Weapon/WeaponLevelData.cs`** *(Tạo mới)*:
   - **Nội dung:** Class dữ liệu chi tiết cho từng cấp độ vũ khí: `damage`, `cooldown`, `projectileSpeed`, `projectileCount`, `areaScale`, `duration`, `firePattern`, `motionType`, `spreadAngle`, `spinSpeed`, `waveFrequency`, `waveAmplitude`, `upgradeDescription`.
   - **Lý do:** Giúp mỗi cấp độ có thể tăng các chỉ số khác nhau (ví dụ Lv.2 +1 đạn, Lv.3 giảm hồi chiêu, Lv.4 đạn vừa bay vừa xoay).
4. **`Assets/_ScriptsObject/Weapon/WeaponEvolutionData.cs`** *(Tạo mới)*:
   - **Nội dung:** ScriptableObject chứa công thức tiến hóa: `baseWeapon`, `requiredPassive`, `requiredWeaponLevel`, `evolvedWeapon`, `evolvedWeaponPrefab`, và hàm kiểm tra `CanEvolve()`.
   - **Lý do:** Chuẩn bị sẵn cơ chế kết hợp Vũ khí Max Level + Passive để kích hoạt vũ khí tối thượng (Giai đoạn 3).
5. **`Assets/_ScriptsObject/Weapon/WeaponData.cs`** *(Chỉnh sửa)*:
   - **Thay đổi:** Thêm `weaponName`, `baseDescription`, danh sách `levels` (`List<WeaponLevelData>`), cờ `isEvolution`, liên kết `evolutionData`. Đồng thời **giữ nguyên toàn bộ biến cũ** (`projectileKey`, `damage`, `cooldown`, `projectileSpeed`, `weaponSprite`) làm fallback.
   - **Lý do:** Tương thích ngược $100\%$ với các file asset hiện có (`Knife.asset`, `Axe.asset`, v.v.), không làm mất dữ liệu cũ và tự động sinh stats hợp lý nếu chưa kịp nhập levels trong Editor.

---

### ⚙️ B. Tầng Logic Chiến Đấu & Vũ Khí
1. **`Assets/_Scripts/Weapons/WeaponController.cs`** *(Tái cấu trúc toàn diện)*:
   - **Thay đổi:** 
     - Áp dụng Template Method Pattern: Lớp cha tự chạy `Update()`, đếm `timer`, so sánh `cooldown` và gọi hàm ảo `Attack()`.
     - Quản lý `currentLevel`, `MaxLevel`, `IsMaxLevel`, `UpgradeWeapon()`.
     - Thiết lập hệ thống tính **Final Stats** (`GetFinalDamage()`, `GetFinalCooldown()`, `GetFinalProjectileCount()`, `GetFinalAreaScale()`, `GetFinalSpeed()`, `GetFinalDuration()`).
     - Tích hợp các hàm bắn đạn linh hoạt: `ExecuteFirePattern()`, `SpawnSingleProjectile()`, `GetAimDirection()`.
   - **Lý do:** Xóa bỏ hoàn toàn code lặp lại ở 6 vũ khí con; giúp vũ khí con chỉ cần tập trung vào đúng logic xuất chiêu đặc thù. Đồng thời tạo "ổ cắm" sẵn cho Passive Items tự scale chỉ số.
2. **`Assets/_Scripts/Weapons/PlayerWeapon/Knife/Projectile.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Bổ sung hàm `InitializeAdvanced()` hỗ trợ đa dạng chuyển động (`Straight`, `Spinning`, `SineWave`, `Boomerang`, `Homing`) và scale kích thước đạn theo `areaScale`.
   - **Lý do:** Đạn có thể biến hóa quỹ đạo tự do (lúc bay thẳng, lúc xoay tít, lúc lượn sóng) theo cấu hình từ ScriptableObject.
3. **`Assets/_Scripts/Weapons/PlayerWeapon/Knife/KnifeWeaponController.cs`** *(Rút gọn)*:
   - **Thay đổi:** Giảm từ 48 dòng xuống còn 15 dòng. Kế thừa `WeaponController` và gọi `ExecuteFirePattern(dir)`.
   - **Lý do:** Tự động hỗ trợ bắn rẻ quạt (Fan Spread) khi số lượng đạn $> 1$, hỗ trợ mọi quỹ đạo bay từ SO.
4. **`Assets/_Scripts/Weapons/PlayerWeapon/Axe/AxeWeaponController.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Hỗ trợ ném nhiều rìu tỏa đều các hướng theo `GetFinalProjectileCount()`, phóng to rìu theo `GetFinalAreaScale()`.
   - **Lý do:** Cấp càng cao ném càng nhiều rìu quét sạch quái xung quanh người chơi.
5. **`Assets/_Scripts/Weapons/PlayerWeapon/Magic/MagicOrbWeapon.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Phân bổ $N$ quả cầu ma thuật chia đều cho $N$ quái trong tầm quét. Nếu chỉ có 1 quái/Boss thì dồn toàn bộ quả cầu vào mục tiêu đó.
   - **Lý do:** Tăng khả năng dọn đàn quái và biến Magic Orb thành vũ khí dồn sát thương đơn mục tiêu cực mạnh khi đánh Boss.
6. **`Assets/_Scripts/Weapons/PlayerWeapon/Whip/WhipController.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Scale kích thước vùng quét theo `GetFinalAreaScale()`. Khi có từ 2 đòn trở lên: quất 1 đòn phía trước + 1 đòn **bọc hậu phía sau lưng**.
   - **Lý do:** Tái hiện cơ chế roi kinh điển của Vampire Survivors, bảo vệ người chơi khỏi bị móc lốp.
7. **`Assets/_Scripts/Weapons/PlayerWeapon/Lightning/LightningController.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Cho phép đánh đồng thời nhiều cột sét (`initialStrikes = GetFinalProjectileCount()`), mở rộng tầm giật lan theo `GetFinalAreaScale()`.
   - **Lý do:** Tạo cảm giác bão sấm sét bao phủ màn hình ở các cấp độ cao.
8. **`Assets/_Scripts/Weapons/PlayerWeapon/Shield/ShieldWeaponController.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Phóng to vòng bảo vệ theo `GetFinalAreaScale()`, kéo dài thời gian kích hoạt theo `GetFinalDuration()` và cập nhật sát thương theo cấp.
   - **Lý do:** Vòng khiên to dần và thời gian duy trì liên tục hơn khi nâng cấp.

---

### 🖥️ C. Tầng Giao Diện & Logic Bốc Thưởng
1. **`Assets/_Scripts/UpgradeOptionUI.cs`** *(Nâng cấp)*:
   - **Thay đổi:** Bổ sung hiển thị `textName` (Tên vũ khí), `textLevel` (`MỚI!` nếu chưa có, hoặc `Cấp 1 ➔ 2`), và `textDescription` (Mô tả chi tiết của cấp tiếp theo). Có null-check an toàn để không bắt buộc prefab phải có sẵn text.
   - **Lý do:** Cung cấp đầy đủ thông tin cho người chơi đưa ra quyết định khi lên cấp.
2. **`Assets/_Scripts/UIManager.cs`** *(Chỉnh sửa)*:
   - **Thay đổi:** 
     - Lọc danh sách quay thưởng: **Loại bỏ các vũ khí đã đạt cấp tối đa (`IsMaxLevel`)** khỏi pool nâng cấp.
     - Hàm `UpgradeWeapon()` gọi trực tiếp `ctrl.UpgradeWeapon()` để tăng cấp và áp dụng toàn bộ stats mới.
   - **Lý do:** Ngăn chặn việc người chơi bốc phải món đồ đã tối đa cấp, đảm bảo luồng chơi chuẩn Rogue-lite.

---

## 🚀 3. Hướng Dẫn Cấu Hình Thử Nghiệm Trong Unity Editor

1. Chọn file ScriptableObject của Knife tại: `Assets/_ScriptsObject/Weapon/Knife.asset`.
2. Trong Inspector, mở rộng danh sách `Levels` (nhấn `+` để thêm các cấp độ):
   - **Level 1**: `projectileCount = 1`, `firePattern = Forward`, `motionType = Straight`, `upgradeDescription = "Phi đao bay thẳng cơ bản"`.
   - **Level 2**: `projectileCount = 2`, `spreadAngle = 15`, `firePattern = FanSpread`, `motionType = Straight`, `upgradeDescription = "+1 Phi đao (Bắn chùm 2 dao)"`.
   - **Level 3**: `projectileCount = 3`, `firePattern = FanSpread`, `motionType = Spinning`, `spinSpeed = 720`, `upgradeDescription = "+1 Phi đao, dao vừa bay vừa xoay tròn xé gió!"`.
   - **Level 4**: `projectileCount = 4`, `firePattern = FanSpread`, `motionType = SineWave`, `waveFrequency = 10`, `waveAmplitude = 1.5`, `upgradeDescription = "+1 Phi đao, quỹ đạo lượn sóng diện rộng!"`.
3. Nhấn Play trong Unity, tiêu diệt quái nhặt EXP để lên cấp và trải nghiệm sự biến đổi hình thái tấn công của vũ khí!

---

---

## 💎 4. CẬP NHẬT GIAI ĐOẠN 2: HỆ THỐNG PASSIVE ITEMS (TRANG BỊ PHỤ TRỢ)
**Trạng thái:** ✅ ĐÃ HOÀN THÀNH

### Các thành phần đã triển khai:
1. **`Assets/_Scripts/Player/PlayerPassiveManager.cs`**:
   - Quản lý danh sách các trang bị phụ trợ người chơi đang sở hữu (`maxPassiveSlots = 3`).
   - Cung cấp hàm `GetBonus(statType)` tính toán tổng % hoặc số lượng chỉ số tăng thêm.
   - Cơ chế hồi máu theo giây (`Recovery`) tự động kích hoạt trong `Update()`.
   - Cơ chế **Fail-safe Auto-Attach**: Trong `Player.Awake()`, nếu GameObject Player chưa có component `PlayerPassiveManager`, script sẽ tự động `AddComponent` để người dùng không cần phải gán thủ công trong Editor.
2. **Kết nối trực tiếp vào `WeaponController.cs`**:
   - `GetFinalDamage()`: Tự động nhân thêm % bonus từ `PassiveStatType.Attack`.
   - `GetFinalCooldown()`: Tự động giảm % hồi chiêu từ `PassiveStatType.Cooldown`.
   - `GetFinalSpeed()`: Tự động tăng tốc bay từ `PassiveStatType.BulletSpeed`.
   - `GetFinalProjectileCount()`: Tự động cộng thêm số lượng đạn từ `PassiveStatType.AmountBullet`.
   - `GetFinalAreaScale()`: Tự động tăng kích thước đạn/vùng quét từ `PassiveStatType.Area`.
3. **Kết nối trực tiếp vào `Player.cs` & `PlayerController.cs`**:
   - `Player.TakeDamage()`: Tự động trừ đi điểm Giáp từ `PassiveStatType.Armor`.
   - `Player.UpdateMaxHealthBonus()`: Tăng máu tối đa từ `PassiveStatType.MaxHealth` và cộng dồn lượng máu chênh lệch.
   - `PlayerController.UpdateMoveSpeedBonus()`: Tăng tốc độ di chuyển từ `PassiveStatType.MoveSpeed`.
4. **Nâng cấp Giao diện Nâng Cấp (`UpgradeOptionUI` & `UIManager`)**:
   - `UpgradeOptionUI.cs`: Hỗ trợ cả `SetupWeapon(weapon)` lẫn `SetupPassive(passiveData)`.
   - `UIManager.cs`: Bổ sung danh sách `allPassives` vào pool bốc thưởng. Người chơi khi lên cấp sẽ nhận được ngẫu nhiên cả Vũ khí và Passive Item. Tự động loại bỏ các món đã đạt cấp tối đa (`IsMaxLevel`).

---

## 📌 5. Kế Hoạch Tiếp Theo: GIAI ĐOẠN 3 (TIẾN HÓA VŨ KHÍ)
1. **Kích hoạt Tiến Hóa**:
   - Khi mở rương báu (`Treasure.cs`) hoặc khi bốc thưởng: Kiểm tra điều kiện Player có Vũ khí Lv.8 (Max) + Passive tương ứng.
2. **Thay thế / Biến đổi Vũ Khí**:
   - Thay thế GameObject vũ khí thường bằng vũ khí tiến hóa tối thượng (ví dụ: *Knife + Bracer* ➔ *Thousand Edge*).

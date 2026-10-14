# 🏛️ SƠ ĐỒ THIẾT KẾ KIẾN TRÚC UML (WEAPON & PASSIVE UPGRADE SYSTEM)
**Dự án:** 2D Top-Down Roguelike (Vampire Survivors-like)  
**Ngày cập nhật:** 14/10/2026  

Tài liệu này cung cấp các sơ đồ UML chi tiết (Class Diagram, Sequence Diagram, Activity Diagram) mô tả toàn bộ cấu trúc hệ thống Vũ khí (Weapons), Trang bị phụ trợ (Passive Items), Quỹ đạo bay (Trajectories) và Vòng lặp Nâng cấp (Level Up Loop).

---

## 1. 📊 Sơ Đồ Lớp Tổng Thể (Class Diagram)

Sơ đồ thể hiện mối quan hệ giữa 4 tầng chính: **Data Layer (ScriptableObjects)**, **Combat Layer (Weapons & Projectiles)**, **Player & Passive Layer**, và **UI Layer (Level Up System)**.

```mermaid
classDiagram
    %% ================= TẦNG DATA (SCRIPTABLE OBJECTS) =================
    class WeaponData {
        +string weaponName
        +Sprite icon
        +string projectileKey
        +int damage
        +float cooldown
        +float projectileSpeed
        +List~WeaponLevelData~ levels
        +bool isEvolution
        +WeaponEvolutionData evolutionData
        +GetLevelData(int level) WeaponLevelData
        +GetUpgradeDescription(int level) string
    }

    class WeaponLevelData {
        +int damage
        +float cooldown
        +float projectileSpeed
        +int projectileCount
        +float areaScale
        +float duration
        +FirePattern firePattern
        +ProjectileMotionType motionType
        +float spreadAngle
        +float spinSpeed
        +float waveFrequency
        +float waveAmplitude
        +string upgradeDescription
    }

    class WeaponEvolutionData {
        +WeaponData baseWeapon
        +PassiveItemData requiredPassive
        +int requiredWeaponLevel
        +WeaponData evolvedWeapon
        +GameObject evolvedWeaponPrefab
        +CanEvolve(int wLevel, int pLevel) bool
    }

    class PassiveItemData {
        +string passiveName
        +Sprite icon
        +string baseDescription
        +PassiveStatType statType
        +List~PassiveLevelData~ levels
        +GetLevelData(int level) PassiveLevelData
        +GetUpgradeDescription(int level) string
    }

    class PassiveLevelData {
        +float Value
        +string upgradeDescription
    }

    %% QUAN HỆ DATA
    WeaponData *-- WeaponLevelData : Chứa danh sách (Composition)
    WeaponData o-- WeaponEvolutionData : Liên kết tiến hóa
    WeaponEvolutionData o-- WeaponData : Vũ khí gốc & Sau tiến hóa
    WeaponEvolutionData o-- PassiveItemData : Passive yêu cầu
    PassiveItemData *-- PassiveLevelData : Chứa danh sách

    %% ================= TẦNG CHIẾN ĐẤU (COMBAT & WEAPONS) =================
    class WeaponController {
        <<abstract>>
        #WeaponData weaponData
        #Transform spawnHolder
        +int currentLevel
        +FirePattern currentFirePattern
        +ProjectileMotionType currentMotionType
        +ApplyLevelStats(int level)
        +UpgradeWeapon()
        +GetFinalDamage() int
        +GetFinalCooldown() float
        +GetFinalSpeed() float
        +GetFinalProjectileCount() int
        +GetFinalAreaScale() float
        #ExecuteFirePattern(Vector2 dir)
        #SpawnSingleProjectile(Vector2 pos, Vector2 dir)
        #Attack()*
    }

    class KnifeWeaponController {
        #Attack()
    }
    class AxeWeaponController {
        #Attack()
    }
    class MagicOrbWeapon {
        #Attack()
    }
    class WhipController {
        #Attack()
    }
    class LightningController {
        #Attack()
    }
    class ShieldWeaponController {
        #Attack()
    }

    WeaponController <|-- KnifeWeaponController : Kế thừa
    WeaponController <|-- AxeWeaponController : Kế thừa
    WeaponController <|-- MagicOrbWeapon : Kế thừa
    WeaponController <|-- WhipController : Kế thừa
    WeaponController <|-- LightningController : Kế thừa
    WeaponController <|-- ShieldWeaponController : Kế thừa
    WeaponController o-- WeaponData : Tham chiếu dữ liệu

    class Projectile {
        -ProjectileMotionType motionType
        -float speed
        -int damage
        +InitializeAdvanced(...)
        -UpdateMotion()
        -HandleDamage(GameObject target)
        -DespawnSelf()
    }

    WeaponController ..> Projectile : Spawn đạn từ Pool

    %% ================= TẦNG PASSIVE & PLAYER =================
    class PlayerPassiveManager {
        -List~PassiveSlot~ ownedPassives
        +GetBonus(PassiveStatType statType) float
        +AddOrUpgradePassive(PassiveItemData passive)
        +HasPassive(PassiveItemData passive) bool
        +CanAddPassive() bool
        +ApplyPassiveStatsToPlayer()
    }

    class PassiveSlot {
        +PassiveItemData data
        +int currentLevel
        +IsMaxLevel bool
    }

    class Player {
        -int Health
        -int MaxHealth
        +TakeDamage(int dame)
        +Heal(int amount)
        +UpdateMaxHealthBonus(float bonus)
    }

    class PlayerController {
        -float MoveSpeed
        +UpdateMoveSpeedBonus(float bonus)
    }

    PlayerPassiveManager *-- PassiveSlot : Quản lý các slot
    PassiveSlot o-- PassiveItemData : Tham chiếu Data
    WeaponController ..> PlayerPassiveManager : Đọc Bonus chỉ số toàn cục
    PlayerPassiveManager --> Player : Cập nhật Máu / Giáp / Hồi máu
    PlayerPassiveManager --> PlayerController : Cập nhật Tốc độ di chuyển

    %% ================= TẦNG GIAO DIỆN & NÂNG CẤP (UI) =================
    class UIManager {
        -List~GameObject~ allWeapons
        -List~PassiveItemData~ allPassives
        -List~GameObject~ currentWeapons
        +Upgrade()
        +PickWeapon(GameObject weapon)
        +PickPassive(PassiveItemData passive)
    }

    class UpgradeOptionUI {
        -Image weaponImage
        -Button pickButton
        -TextMeshProUGUI textName
        -TextMeshProUGUI textLevel
        -TextMeshProUGUI textDescription
        +SetupWeapon(GameObject weapon)
        +SetupPassive(PassiveItemData passive)
        -Pick()
    }

    UIManager o-- UpgradeOptionUI : Điều khiển hiển thị danh sách thẻ
    UpgradeOptionUI ..> UIManager : Gọi lệnh Pick lựa chọn
    UIManager ..> WeaponController : Nâng cấp vũ khí
    UIManager ..> PlayerPassiveManager : Nâng cấp passive
```

---

## 2. ⏱️ Sơ Đồ Trình Tự Bốc Thưởng Khi Lên Cấp (Sequence Diagram)

Mô tả luồng tương tác giữa Người chơi, EventBus, UIManager, PlayerPassiveManager và WeaponController:

```mermaid
sequenceDiagram
    autonumber
    actor Player as Người chơi
    participant PExp as PlayerExp
    participant UIM as UIManager
    participant PassMgr as PlayerPassiveManager
    participant OptUI as UpgradeOptionUI
    participant WC as WeaponController

    Player->>PExp: Nhặt ngọc EXP đạt ngưỡng lên cấp
    PExp->>UIM: Bắn sự kiện OnLevelUpEvent
    Note over UIM: Time.timeScale = 0 (Tạm dừng game)
    UIM->>UIM: Lọc các Weapon chưa Max Cấp
    UIM->>PassMgr: Lọc các Passive chưa Max Cấp
    UIM->>OptUI: SetupWeapon() / SetupPassive() (3 thẻ ngẫu nhiên)
    UIM->>Player: Hiển thị Panel Nâng Cấp

    Player->>OptUI: Nhấp nút "Pick" lựa chọn

    alt Người chơi chọn Vũ Khí (Weapon)
        OptUI->>UIM: PickWeapon(weapon)
        alt Nếu vũ khí đã sở hữu
            UIM->>WC: UpgradeWeapon() (currentLevel++)
            WC->>WC: ApplyLevelStats(newLevel)
        else Nếu là vũ khí mới
            UIM->>UIM: AddWeapon() (SetActive = true)
        end
    else Người chơi chọn Trang Bị Phụ Trợ (Passive)
        OptUI->>UIM: PickPassive(passiveData)
        UIM->>PassMgr: AddOrUpgradePassive(passiveData)
        PassMgr->>PassMgr: Tăng cấp slot & Tính lại Bonus
        PassMgr->>WC: Toàn bộ vũ khí tự động scale Final Stats
    end

    UIM->>Player: Đóng Panel & Time.timeScale = 1 (Tiếp tục game)
```

---

## 3. 🔄 Sơ Đồ Vòng Đời Tấn Công & Quỹ Đạo Đạn (Combat & Trajectory Flow)

```mermaid
flowchart TD
    Start([Update Loop]) --> CheckTimer{Timer >= FinalCooldown?}
    CheckTimer -- Chưa đến giờ --> Wait[Timer += Time.deltaTime]
    CheckTimer -- Đã hồi chiêu --> ResetTimer[Timer = 0]
    
    ResetTimer --> ReadStats[Đọc Chỉ Số Cuối Cùng]
    ReadStats --> Dmg["Damage = BaseDamage * (1 + PassiveAttackBonus)"]
    ReadStats --> Amount["Count = BaseCount + PassiveAmountBonus"]
    ReadStats --> Area["AreaScale = BaseArea * (1 + PassiveAreaBonus)"]
    
    Dmg & Amount & Area --> CallAttack[Gọi hàm Attack của Vũ Khí Con]
    
    CallAttack --> PatternCheck{Kiểu bắn FirePattern}
    PatternCheck -- Forward --> P1[Bắn song song dàn hàng ngang phía trước]
    PatternCheck -- FanSpread --> P2["Bắn tỏa góc rẻ quạt (-15° đến +15°)"]
    PatternCheck -- Radial360 --> P3[Bắn tỏa tròn 360° quanh Player]
    PatternCheck -- Burst --> P4[Bắn liên thanh nhiều đợt theo hướng nhìn]
    
    P1 & P2 & P3 & P4 --> SpawnProj[PoolManager.Spawn Projectile]
    
    SpawnProj --> MotionCheck{Quỹ đạo MotionType}
    MotionCheck -- Straight --> M1[Bay thẳng tắp]
    MotionCheck -- Spinning --> M2[Vừa bay vừa xoay tròn xé gió]
    MotionCheck -- SineWave --> M3[Uốn lượn hình Sin]
    MotionCheck -- Boomerang --> M4[Bay xa rồi lượn về chủ nhân]
    MotionCheck -- Homing --> M5[Tự bẻ lái bám đuổi quái]
```

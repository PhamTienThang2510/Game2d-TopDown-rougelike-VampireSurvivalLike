using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PassiveSlot
{
    public PassiveItemData data;
    public int currentLevel = 1;

    public bool IsMaxLevel => data != null && currentLevel >= data.MaxLevel;

    public PassiveSlot(PassiveItemData data, int level = 1)
    {
        this.data = data;
        this.currentLevel = level;
    }
}

public class PlayerPassiveManager : MonoBehaviour
{
    public static PlayerPassiveManager Instance { get; private set; }

    [Header("Slot Settings")]
    [SerializeField] private int maxPassiveSlots = 3;

    [Header("Current Passives")]
    [SerializeField] private List<PassiveSlot> ownedPassives = new();

    private Player player;
    private PlayerController playerController;
    private float recoveryTimer = 0f;

    public int MaxPassiveSlots => maxPassiveSlots;
    public IReadOnlyList<PassiveSlot> OwnedPassives => ownedPassives;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        EventBus<OnPassiveSelectedEvent>.Subscribe(OnPassiveSelected);
    }

    private void OnDisable()
    {
        EventBus<OnPassiveSelectedEvent>.Unsubscribe(OnPassiveSelected);
    }

    private void OnPassiveSelected(OnPassiveSelectedEvent evt)
    {
        if (evt.Passive != null)
        {
            AddOrUpgradePassive(evt.Passive);
        }
    }

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<Player>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        ApplyPassiveStatsToPlayer();
    }

    private void Update()
    {
        HandleHealthRecovery();
    }

    /// <summary>
    /// Lọc danh sách các Passive có thể xuất hiện trong bảng nâng cấp
    /// </summary>
    public List<PassiveItemData> GetAvailablePassiveUpgrades(List<PassiveItemData> allPassives)
    {
        List<PassiveItemData> available = new();
        if (allPassives == null) return available;

        bool canAdd = CanAddPassive();

        foreach (var p in allPassives)
        {
            if (p == null) continue;
            if (IsMaxLevel(p)) continue;

            if (HasPassive(p) || canAdd)
            {
                available.Add(p);
            }
        }

        return available;
    }

    /// <summary>
    /// Hồi máu theo thời gian nếu có chỉ số Recovery
    /// </summary>
    private void HandleHealthRecovery()
    {
        float recoveryRate = GetBonus(PassiveStatType.Recovery);
        if (recoveryRate <= 0f || player == null || player.IsDead) return;

        recoveryTimer += Time.deltaTime;
        if (recoveryTimer >= 1f)
        {
            recoveryTimer -= 1f;
            int healAmount = Mathf.Max(1, Mathf.RoundToInt(recoveryRate));
            player.Heal(healAmount);
        }
    }

    /// <summary>
    /// Lấy tổng giá trị cộng thêm của một loại chỉ số từ tất cả Passive đang sở hữu
    /// </summary>
    public float GetBonus(PassiveStatType statType)
    {
        float total = 0f;
        for (int i = 0; i < ownedPassives.Count; i++)
        {
            var slot = ownedPassives[i];
            if (slot.data != null && slot.data.statType == statType)
            {
                var lvlData = slot.data.GetLevelData(slot.currentLevel);
                if (lvlData != null)
                {
                    total += lvlData.Value;
                }
            }
        }
        return total;
    }

    /// <summary>
    /// Kiểm tra người chơi đã sở hữu Passive này chưa
    /// </summary>
    public bool HasPassive(PassiveItemData passive)
    {
        if (passive == null) return false;
        return ownedPassives.Exists(p => p.data == passive);
    }

    /// <summary>
    /// Lấy cấp độ hiện tại của một Passive (trả về 0 nếu chưa sở hữu)
    /// </summary>
    public int GetPassiveLevel(PassiveItemData passive)
    {
        if (passive == null) return 0;
        var slot = ownedPassives.Find(p => p.data == passive);
        return slot != null ? slot.currentLevel : 0;
    }

    /// <summary>
    /// Kiểm tra Passive đã đạt cấp độ tối đa chưa
    /// </summary>
    public bool IsMaxLevel(PassiveItemData passive)
    {
        if (passive == null) return true;
        var slot = ownedPassives.Find(p => p.data == passive);
        return slot != null && slot.IsMaxLevel;
    }

    /// <summary>
    /// Kiểm tra xem còn slot trống để thêm Passive mới không
    /// </summary>
    public bool CanAddPassive()
    {
        return ownedPassives.Count < maxPassiveSlots;
    }

    /// <summary>
    /// Thêm mới hoặc nâng cấp một Passive Item
    /// </summary>
    public void AddOrUpgradePassive(PassiveItemData passive)
    {
        if (passive == null) return;

        var slot = ownedPassives.Find(p => p.data == passive);
        if (slot != null)
        {
            if (!slot.IsMaxLevel)
            {
                slot.currentLevel++;
            }
        }
        else
        {
            if (CanAddPassive())
            {
                ownedPassives.Add(new PassiveSlot(passive, 1));
            }
        }

        ApplyPassiveStatsToPlayer();
    }

    /// <summary>
    /// Áp dụng các chỉ số thay đổi trực tiếp lên Player (Tốc độ chạy, Máu tối đa...)
    /// </summary>
    public void ApplyPassiveStatsToPlayer()
    {
        // 1. Tốc độ di chuyển
        if (playerController != null)
        {
            float moveBonus = GetBonus(PassiveStatType.MoveSpeed);
            playerController.UpdateMoveSpeedBonus(moveBonus);
        }

        // 2. Máu tối đa
        if (player != null)
        {
            float maxHealthBonus = GetBonus(PassiveStatType.MaxHealth);
            player.UpdateMaxHealthBonus(maxHealthBonus);
        }
    }
}

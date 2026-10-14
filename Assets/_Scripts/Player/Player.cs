using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private PlayerController controller;
    [Header("Health Settings")]
    [SerializeField] private int Health;
    [SerializeField] private int MaxHealth = 20;
    private int baseMaxHealth = 20;

    [Header("Damage Settings")]
    [SerializeField] private float damageDelay = 2f;
    private Dictionary<IDameSource, float> lastDamageTime = new();
    public bool IsDead = false;

    private void Awake()
    {
        baseMaxHealth = MaxHealth;
        this.Health = MaxHealth;

        if (!TryGetComponent<PlayerPassiveManager>(out _))
        {
            gameObject.AddComponent<PlayerPassiveManager>();
        }
    }
    void Start()
    {
        controller = GetComponent<PlayerController>();
        EventBus<OnHealthChangedEvent>.Publish(new OnHealthChangedEvent(Health, MaxHealth));
    }

    public void UpdateMaxHealthBonus(float bonusPercent)
    {
        int newMax = baseMaxHealth + Mathf.RoundToInt(baseMaxHealth * bonusPercent);
        if (newMax != MaxHealth)
        {
            int diff = newMax - MaxHealth;
            MaxHealth = newMax;
            if (diff > 0) Health += diff;
            EventBus<OnHealthChangedEvent>.Publish(new OnHealthChangedEvent(Health, MaxHealth));
        }
    }

    public void TakeDamage(int dame)
    {
        int armor = (PlayerPassiveManager.Instance != null)
            ? Mathf.RoundToInt(PlayerPassiveManager.Instance.GetBonus(PassiveStatType.Armor))
            : 0;
        int finalDamage = Mathf.Max(1, dame - armor);

        this.Health -= finalDamage;
        EventBus<OnHealthChangedEvent>.Publish(new OnHealthChangedEvent(Health, MaxHealth));
        if (Health <= 0) PlayerDie();
    }
    public void Heal(int amount)
    {
        this.Health += amount;
        if (this.Health > MaxHealth)
            this.Health = MaxHealth;
        EventBus<OnHealthChangedEvent>.Publish(new OnHealthChangedEvent(Health, MaxHealth));
    }
    private void PlayerDie()
    {
        controller.PlayerDeath();
        EventBus<OnGameOverEvent>.Publish();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out IDameSource source))
            return;

        // nếu lần đầu va chạm
        if (!lastDamageTime.ContainsKey(source))
        {
            TakeDamage(source.SendDame());              // damage ngay
            lastDamageTime[source] = Time.time;         // lưu mốc thời gian
            return;
        }

        // các lần tiếp theo
        if (Time.time - lastDamageTime[source] >= damageDelay)
        {
            TakeDamage(source.SendDame());
            lastDamageTime[source] = Time.time;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IDameSource source))
        {
            lastDamageTime.Remove(source);
        }
    }

}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable
{
    public event Action OnStateGameOver;
    [Header("Components")]
    [SerializeField] private PlayerController controller;
    [Header("Health Settings")]
    [SerializeField] private int Health;
    [SerializeField] private int MaxHealth = 20;
    [Header("Damage Settings")]
    [SerializeField] private float damageDelay = 2f;
    private Dictionary<IDameSource, float> lastDamageTime = new();
    public bool IsDead = false;

    private void Awake()
    {
        this.Health = MaxHealth;
    }
    void Start()
    {
        controller = GetComponent<PlayerController>();
        UIManager.instance.SettingHealthUI(Health, MaxHealth);
    }

    public void TakeDamage(int dame)
    {
        this.Health -= dame;
        UIManager.instance.SettingHealthUI(Health, MaxHealth);
        if (Health <= 0) PlayerDie();
    }
    public void Heal(int amount)
    {
        this.Health += amount;
        if (this.Health > MaxHealth)
            this.Health = MaxHealth;
        UIManager.instance.SettingHealthUI(Health, MaxHealth);
    }
    private void PlayerDie()
    {
        controller.PlayerDeath();
        OnStateGameOver?.Invoke();
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

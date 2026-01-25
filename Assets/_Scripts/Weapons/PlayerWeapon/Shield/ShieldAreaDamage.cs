using System.Collections.Generic;
using UnityEngine;

public class ShieldAreaDamage : MonoBehaviour
{
    [SerializeField] private int dame;
    [SerializeField] private float damageInterval;

    private Dictionary<IDamageable, float> damageTimers = new();

    public void Initialize(int dame, float damageInterval)
    {
        this.dame = dame;
        this.damageInterval = damageInterval;
        damageTimers.Clear();
    }

    public void UpdateDamage(int dame)
    {
        this.dame = dame;
    }

    private void Update()
    {
        var enemies = new List<IDamageable>(damageTimers.Keys);

        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                damageTimers.Remove(enemy);
                continue;
            }

            damageTimers[enemy] += Time.deltaTime;

            if (damageTimers[enemy] >= damageInterval)
            {
                enemy.TakeDamage(dame);
                damageTimers[enemy] -= damageInterval; // chuẩn hơn reset = 0
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(CONSTANT.ENEMY_TAG)) return;

        IDamageable enemy = other.GetComponent<IDamageable>();
        if (enemy == null) return;

        if (!damageTimers.ContainsKey(enemy))
        {
            enemy.TakeDamage(dame);        // ✅ HIT NGAY KHI VÀO
            damageTimers.Add(enemy, 0f);   // bắt đầu đếm cho hit tiếp theo
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IDamageable enemy = other.GetComponent<IDamageable>();
        if (enemy != null)
            damageTimers.Remove(enemy);
    }
}

using UnityEngine;
using System;
public class Slime_Base : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] protected EnemySO enemySO;
    [SerializeField] protected int Health;
    [SerializeField] protected int MaxHealth;
    [SerializeField] protected int Damage;
    [Header("Damage Text Settings")]
    [SerializeField] protected Vector3 damageTextOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] protected bool isDead;
    [SerializeField] public SlimeMovementBase slimeMovement;
    [Header("Action")]
    public static Action<Vector2> OnSlimeDie;
    public static Action<int> OnSlimeHitPlayer;
    public virtual void UpgradeDataSlime(int healthMultiplier, int damageMultiplier)
    {
        //function to override in child class
    }
    private void OnDisable()
    {
        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.UnregisterEnemy();
    }

}

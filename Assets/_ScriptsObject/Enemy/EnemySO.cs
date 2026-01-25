using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    [SerializeField] public int MaxHealth;
    [SerializeField] public int damage;
    [SerializeField] public int AttackTime;
}

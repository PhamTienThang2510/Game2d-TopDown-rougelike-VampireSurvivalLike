using UnityEngine;

public class SlimeMovementBase : MonoBehaviour
{
    [Header("Slime Movement")]
    [SerializeField] protected float moveSpeed = 5f;
    [Header("Knockback")]
    [SerializeField] protected float knockbackDistance = 0.2f;
    [SerializeField] protected float knockbackDuration = 0.1f;
    [Header("references to target ")]
    public Transform target;
    protected bool isKnockback;
    [Header("Sprite")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    public virtual void Knockback(Vector2 hitDirection)
    {
        //function to override in child class
    }
}

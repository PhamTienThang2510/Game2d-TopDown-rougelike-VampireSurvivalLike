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

    protected virtual void OnEnable()
    {
        isKnockback = false;
    }

    protected Vector2 GetFlowMoveDirection(Vector2 currentPosition)
    {
        if (FlowFieldManager.Instance != null)
        {
            return FlowFieldManager.Instance.GetFlowDirection(currentPosition, target);
        }

        if (target != null)
        {
            return ((Vector2)target.position - currentPosition).normalized;
        }

        return Vector2.zero;
    }

    public virtual void Knockback(Vector2 hitDirection)
    {
        //function to override in child class
    }
}

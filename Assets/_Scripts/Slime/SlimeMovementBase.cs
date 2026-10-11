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

    protected bool isStampede;
    protected Vector2 stampedeDirection;
    protected float stampedeSpeedMultiplier = 1f;

    protected float EffectiveMoveSpeed => isStampede ? moveSpeed * stampedeSpeedMultiplier : moveSpeed;

    protected virtual void OnEnable()
    {
        isKnockback = false;
        isStampede = false;
        stampedeSpeedMultiplier = 1f;
    }

    public void EnableStampede(Vector2 direction, float speedMultiplier = 1.75f)
    {
        isStampede = true;
        stampedeDirection = direction.normalized;
        stampedeSpeedMultiplier = Mathf.Max(0.5f, speedMultiplier);
    }

    protected Vector2 GetFlowMoveDirection(Vector2 currentPosition)
    {
        if (isStampede)
        {
            return stampedeDirection;
        }

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

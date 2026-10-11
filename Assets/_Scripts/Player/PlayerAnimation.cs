using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Player Animator")]
    [SerializeField] public Animator Animator;

    private void OnEnable()
    {
        EventBus<OnPlayerMovementStateEvent>.Subscribe(HandleMovementState);
        EventBus<OnPlayerDeathEvent>.Subscribe(HandlePlayerDeath);
    }

    private void OnDisable()
    {
        EventBus<OnPlayerMovementStateEvent>.Unsubscribe(HandleMovementState);
        EventBus<OnPlayerDeathEvent>.Unsubscribe(HandlePlayerDeath);
    }

    private void HandleMovementState(OnPlayerMovementStateEvent evt)
    {
        Animator.SetBool("IsWalk", evt.IsWalking);
    }

    private void HandlePlayerDeath(OnPlayerDeathEvent _)
    {
        Animator.SetBool("IsDeath", true);
    }
}

using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Player Animator")]
    [SerializeField] public Animator Animator;
    [SerializeField] public PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = this.GetComponent<PlayerController>();
        playerController.OnPlayerWalk += HandlePlayerWalk;
        playerController.OnPlayerIdle += HandlePlayerIdle;
        playerController.OnPlayerDeath += HandlePlayerDeath;
    }
    private void HandlePlayerWalk()
    {
        Animator.SetBool("IsWalk", true);
    }
    private void HandlePlayerIdle()
    {
        Animator.SetBool("IsWalk", false);
    }
    private void HandlePlayerDeath()
    {
        Animator.SetBool("IsDeath", true);
    }
}

using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    public event Action OnPlayerWalk;
    public event Action OnPlayerDeath;
    public event Action OnPlayerIdle;
    private bool isWalking = false;

    [Header("Player Rigidbody2D")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float MoveSpeed = 5f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (playerInput == null) return;

        bool isCurrentlyWalking = playerInput.PlayerDirection.sqrMagnitude > 0.01f;

        if (isCurrentlyWalking != isWalking)
        {
            isWalking = isCurrentlyWalking;
            if (isWalking)
            {
                OnPlayerWalk?.Invoke();
            }
            else
            {
                OnPlayerIdle?.Invoke();
            }
        }

        if (playerInput.PlayerDirection.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);   // facing right
        else if (playerInput.PlayerDirection.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);  // facing left

    }
    private void FixedUpdate()
    {
        if (rb != null && playerInput != null)
            rb.linearVelocity = playerInput.PlayerDirection * MoveSpeed;
    }

    public void PlayerDeath()
    {
        OnPlayerDeath?.Invoke();
        this.enabled = false;
        float timewait = 2f;
        Destroy(this.gameObject, timewait);
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public delegate void PlayerSate();
    public PlayerSate OnPlayerWalk;
    public PlayerSate OnPlayerDeath;
    public PlayerSate OnPlayerIdle;
    private bool isWalking = false;

    [Header("Player Rigidbody2D")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float MoveSpeed = 5f;
    public Vector2 PlayerDirection;
    public Vector2 LastMoveDirection = Vector2.right;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        PlayerDirection = new Vector2(inputX, inputY).normalized;
        if (PlayerDirection.sqrMagnitude > 0.01f)
        {
            LastMoveDirection = PlayerDirection.normalized;
        }
        bool isCurrentlyWalking = PlayerDirection.sqrMagnitude > 0f;

        if (isCurrentlyWalking && !isWalking)
        {
            isWalking = true;
            OnPlayerWalk?.Invoke();
        }
        else if (!isCurrentlyWalking && isWalking)
        {
            isWalking = false;
            OnPlayerIdle?.Invoke();
        }

        if (inputX > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);   // facing right
        else if (inputX < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);  // facing left

    }
    private void FixedUpdate()
    {
        if (rb != null)
            rb.linearVelocity = PlayerDirection * MoveSpeed;
    }

    public void PlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }
}

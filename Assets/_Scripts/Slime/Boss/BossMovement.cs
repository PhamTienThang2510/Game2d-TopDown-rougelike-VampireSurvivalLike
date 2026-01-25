using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMovement : SlimeMovementBase
{
    Rigidbody2D rb;
    Animator animator;

    [SerializeField] Transform attackPoint;

    enum State
    {
        Idle,
        DashAttack,
        CircleShot
    }

    [Header("State Timing")]
    [SerializeField] float idleTime = 3f;
    [SerializeField] float dashTime = 2f;
    [SerializeField] float circleShotTime = 4f;

    State currentState;
    float stateTimer;
    bool isBusy;

    [Header("Idle")]
    [SerializeField] float idleSpeed = 2f;

    [Header("Dash")]
    [SerializeField] float dashDistance = 3f;
    [SerializeField] float dashSpeed = 15f;
    [SerializeField] int dashDamage = 10; // damage dealt when passing through player

    [Header("Circle Shot")]
    [SerializeField] string bulletKey;
    [SerializeField] int bulletCount = 12;
    [SerializeField] float shotInterval = 1f;
    float shotTimer;

    // Collision control for dash-through behaviour
    Collider2D bossCollider;
    Collider2D ignoredPlayerCollider;
    bool collisionIgnored;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bossCollider = GetComponent<Collider2D>();
        target = GameObject.FindWithTag(CONSTANT.PLAYER_TAG)?.transform;

        ChangeState(State.Idle);
    }

    void FixedUpdate()
    {
        if (isKnockback || target == null) return;

        stateTimer -= Time.fixedDeltaTime;

        switch (currentState)
        {
            case State.Idle:
                IdleUpdate();
                break;

            case State.DashAttack:
                DashUpdate();
                break;

            case State.CircleShot:
                CircleShotUpdate();
                break;
        }
    }

    #region STATE UPDATE

    void IdleUpdate()
    {
        MoveToPlayer(idleSpeed);

        if (stateTimer <= 0)
            ChangeState(State.DashAttack);
    }

    void DashUpdate()
    {
        if (!isBusy)
            StartCoroutine(DashRoutine());
    }

    void CircleShotUpdate()
    {
        shotTimer -= Time.fixedDeltaTime;

        if (shotTimer <= 0)
        {
            SpawnCircleBullets();
            shotTimer = shotInterval;
        }

        if (stateTimer <= 0)
            ChangeState(State.Idle);
    }

    #endregion

    #region STATE CONTROL

    void ChangeState(State newState)
    {
        // Ensure any temporary collision ignores are restored before changing state
        RestoreCollisionIfNeeded();

        StopAllCoroutines();

        currentState = newState;
        isBusy = false;

        switch (newState)
        {
            case State.Idle:
                stateTimer = idleTime;
                animator.Play("Idle");
                break;

            case State.DashAttack:
                stateTimer = dashTime;
                animator.Play("Boss_Attack_1");
                break;

            case State.CircleShot:
                stateTimer = circleShotTime;
                shotTimer = 0;
                animator.Play("Boss_Attack_2");
                break;
        }
    }

    void RestoreCollisionIfNeeded()
    {
        if (collisionIgnored && bossCollider != null && ignoredPlayerCollider != null)
        {
            Physics2D.IgnoreCollision(bossCollider, ignoredPlayerCollider, false);
        }

        ignoredPlayerCollider = null;
        collisionIgnored = false;
    }

    #endregion

    #region ACTIONS

    IEnumerator DashRoutine()
    {
        isBusy = true;

        Vector2 start = rb.position;
        Vector2 dir = ((Vector2)target.position - start).normalized;
        Vector2 end = start + dir * dashDistance;

        // Temporarily ignore collisions with the player so dash passes through
        if (bossCollider != null && target != null)
        {
            Collider2D playerCol = target.GetComponent<Collider2D>();
            if (playerCol != null)
            {
                ignoredPlayerCollider = playerCol;
                Physics2D.IgnoreCollision(bossCollider, ignoredPlayerCollider, true);
                collisionIgnored = true;
            }
        }

        bool damagedPlayerDuringDash = false;
        float detectRadius = 0.5f;
        if (bossCollider != null)
            detectRadius = Mathf.Max(0.1f, bossCollider.bounds.extents.magnitude);

        while (Vector2.Distance(rb.position, end) > 0.05f)
        {
            Vector2 nextPos = Vector2.MoveTowards(
                rb.position,
                end,
                dashSpeed * Time.fixedDeltaTime);

            rb.MovePosition(nextPos);

            // detect player overlap manually (since collisions are ignored)
            if (!damagedPlayerDuringDash && ignoredPlayerCollider != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, detectRadius);
                for (int i = 0; i < hits.Length; i++)
                {
                    Collider2D hit = hits[i];
                    if (hit == null) continue;
                    if (!hit.CompareTag(CONSTANT.PLAYER_TAG)) continue;

                    if (hit.TryGetComponent<Player>(out var player))
                    {
                        player.TakeDamage(dashDamage);
                        damagedPlayerDuringDash = true;
                        break;
                    }
                }
            }

            spriteRenderer.flipX = dir.x < 0;
            yield return new WaitForFixedUpdate();
        }

        // Wait 1 second before changing state
        yield return new WaitForSeconds(1f);

        // Restore collisions (in case ChangeState was not called from elsewhere)
        RestoreCollisionIfNeeded();

        ChangeState(State.CircleShot);
    }

    void MoveToPlayer(float speed)
    {
        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        spriteRenderer.flipX = dir.x < 0;
    }

    void SpawnCircleBullets()
    {
        float step = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float rad = step * i * Mathf.Deg2Rad;
            Vector2 dir = new(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject go = PoolManager.Instance.Spawn(bulletKey, attackPoint.position);
            if (go && go.TryGetComponent(out Enemy_Attack atk))
                atk.Init(dir);
        }
    }

    #endregion
}

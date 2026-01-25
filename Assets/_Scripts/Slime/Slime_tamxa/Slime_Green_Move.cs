using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Slime_Green_Move : SlimeMovementBase
{
    private Rigidbody2D rb;

    [Header("Follow Settings")]
    [SerializeField] private float keepDistance = 6.5f;
    // removed followDelay / cached snapshot — use player's realtime position

    private bool isMoving;

    private float attackTimer;
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Animator Animator;

    [Header("Attack animation")]
    [Tooltip("How long to hold the attack animation (seconds). If animator uses events, you can set this small or manage via animation events).")]
    [SerializeField] private float attackAnimDuration = 0.6f;

    [Header("Movement smoothing")]
    [Tooltip("Dead zone so slime doesn't jitter when player hovers around keepDistance")]
    [SerializeField] private float distanceTolerance = 0.15f;

    // internal
    private bool isAttacking;
    private Coroutine attackAnimCoroutine;
    private Vector3 initialAttackLocalPos;
    private bool prevFlipX;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Ensure this enemy uses kinematic body so MovePosition isn't affected by physics pushes
        // You can also set this in the prefab inspector; doing it here guarantees runtime behavior.
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.None;
        }

        target = GameObject.FindWithTag(CONSTANT.PLAYER_TAG)?.transform;

        // cache attackPoint local position so we can mirror it when flipping
        if (attackPoint != null)
            initialAttackLocalPos = attackPoint.localPosition;
        else
            initialAttackLocalPos = Vector3.zero;

        prevFlipX = spriteRenderer != null && spriteRenderer.flipX;
    }

    private void Update()
    {
        HandleAttack(); // call attack logic
    }

    private void FixedUpdate()
    {
        if (isKnockback) return;
        UpdateFacingToTarget();
        SlimeMove();
    }

    // ================= FOLLOW =================
    private void UpdateFacingToTarget()
    {
        if (target == null || spriteRenderer == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;

        if (Mathf.Abs(toTarget.x) < 0.01f) return; // tránh rung

        spriteRenderer.flipX = toTarget.x < 0;
        HandleFlipAndAttackPoint();
    }

    private void SlimeMove()
    {
        // only clear attack animation if not currently attacking
        if (!isAttacking)
            Animator?.SetBool("IsAttack", false);

        if (target == null) return;

        Vector2 toTargetRealtime = (Vector2)target.position - rb.position;
        float distance = toTargetRealtime.magnitude;

        float upper = keepDistance + distanceTolerance;
        float lower = keepDistance - distanceTolerance;

        // If player is closer than lower bound -> stand still (do not back away)
        if (distance <= lower)
        {
            isMoving = false;
            HandleFlipAndAttackPoint(); // keep attack point mirrored when standing
            return;
        }

        // If player is within the tolerance band -> stand still and attack
        if (distance <= upper && distance > lower)
        {
            isMoving = false;
            HandleFlipAndAttackPoint();
            return;
        }

        // Player is farther than upper -> move to re-establish keepDistance
        isMoving = true;

        Vector2 dir = toTargetRealtime.normalized;
        Vector2 targetPos = (Vector2)target.position - dir * keepDistance;

        Vector2 newPos = Vector2.MoveTowards(
            rb.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);
        HandleFlipAndAttackPoint();
    }

    // mirror attackPoint local position when sprite flips
    private void HandleFlipAndAttackPoint()
    {
        if (attackPoint == null || spriteRenderer == null) return;

        bool currentFlip = spriteRenderer.flipX;
        if (currentFlip == prevFlipX) return; // nothing to do

        prevFlipX = currentFlip;

        // mirror localPosition.x relative to initial cached value
        float mirroredX = Mathf.Abs(initialAttackLocalPos.x) * (currentFlip ? -1f : 1f);
        attackPoint.localPosition = new Vector3(mirroredX, initialAttackLocalPos.y, initialAttackLocalPos.z);
    }

    // ================= ATTACK =================
    private void HandleAttack()
    {
        if (target == null) return;
        if (isKnockback) return;
        if (isMoving) return; // only shoot when standing still

        attackTimer += Time.deltaTime;
        if (attackTimer < attackCooldown)
            return;

        attackTimer = 0f;
        Shoot();
    }

    private void Shoot()
    {
        // set attack flag & animator
        StartAttackAnim();

        // spawn attack and initialize direction toward player's current position
        var go = PoolManager.Instance.Spawn(CONSTANT.Slime_Green_Attack, attackPoint != null ? attackPoint.position : transform.position);
        if (go != null)
        {
            var enemyAttack = go.GetComponent<Enemy_Attack>();
            if (enemyAttack != null && target != null)
            {
                enemyAttack.Init(((Vector2)target.position - (Vector2)transform.position).normalized);
            }
        }
    }

    private void StartAttackAnim()
    {
        // stop existing coroutine if any
        if (attackAnimCoroutine != null)
            StopCoroutine(attackAnimCoroutine);

        isAttacking = true;
        Animator?.SetBool("IsAttack", true);
        attackAnimCoroutine = StartCoroutine(EndAttackAnimAfter(attackAnimDuration));
    }

    private IEnumerator EndAttackAnimAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isAttacking = false;
        Animator?.SetBool("IsAttack", false);
        attackAnimCoroutine = null;
    }

    // ================= KNOCKBACK =================
    public override void Knockback(Vector2 hitDirection)
    {
        // stop attack animation immediately on knockback
        if (attackAnimCoroutine != null)
        {
            StopCoroutine(attackAnimCoroutine);
            attackAnimCoroutine = null;
        }

        isAttacking = false;
        Animator?.SetBool("IsAttack", false);

        if (isKnockback) return;
        StartCoroutine(KnockbackRoutine(hitDirection));
    }

    private IEnumerator KnockbackRoutine(Vector2 dir)
    {
        isKnockback = true;

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + dir.normalized * knockbackDistance;

        float t = 0f;
        while (t < knockbackDuration)
        {
            t += Time.fixedDeltaTime;
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t / knockbackDuration);
            rb.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }

        isKnockback = false;
    }
}

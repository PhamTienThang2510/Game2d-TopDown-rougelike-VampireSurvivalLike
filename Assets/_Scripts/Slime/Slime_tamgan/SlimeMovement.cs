using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeMovement : SlimeMovementBase
{
    private Rigidbody2D rb;
    private Vector2 moveDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        target = GameObject.FindWithTag(CONSTANT.PLAYER_TAG)?.transform;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (isKnockback) return;
        SlimeMove();
    }

    private void SlimeMove()
    {
        if (target == null) return;
        moveDir = (target.position - transform.position).normalized;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        if (Mathf.Abs(moveDir.x) > 0.001f && spriteRenderer != null)
            spriteRenderer.flipX = moveDir.x < 0;
    }
    public override void Knockback(Vector2 hitDirection)
    {
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

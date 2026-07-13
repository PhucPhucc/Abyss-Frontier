using UnityEngine;

/// <summary>
/// Điều khiển Animator và flip sprite cho PinkMeep.
/// PinkMeep chỉ có asset 1 hướng (Right) — dùng SpriteRenderer.flipX để tạo hướng Left.
/// Kết hợp với EnemyAI để lấy thông tin di chuyển.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(EnemyAI))]
public class PinkMeepAnimatorDriver : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;
    private EnemyAI enemyAI;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        enemyAI = GetComponent<EnemyAI>();
    }

    private void LateUpdate()
    {
        if (enemyAI == null || enemyAI.IsDead) return;

        Vector2 vel = enemyAI.MoveVelocity;

        // Cập nhật isMoving cho Animator
        bool moving = vel.sqrMagnitude > 0.01f;
        anim.SetBool("isMoving", moving);

        // Flip sprite theo hướng ngang
        // moveX > 0 → hướng Right (gốc) → flipX = false
        // moveX < 0 → hướng Left         → flipX = true
        if (vel.x > 0.01f)
            sr.flipX = false;
        else if (vel.x < -0.01f)
            sr.flipX = true;
        // Khi đứng yên (vel.x ≈ 0): giữ nguyên hướng trước đó
    }
}

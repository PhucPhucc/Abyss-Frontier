using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý hiệu ứng knockback (đẩy lùi) và trạng thái choáng (stun) cho Entity.
/// Hoạt động dựa trên Rigidbody2D, giảm dần vận tốc theo thời gian.
/// </summary>
public class KnockbackHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGettingKnockedBack = false; // Đang bị đẩy lùi
    private float knockbackTimer = 0f;          // Bộ đếm thời gian knockback còn lại
    private float knockbackDuration = 0f;       // Tổng thời gian knockback
    private Vector2 knockbackVelocity;           // Vận tốc đẩy lùi ban đầu

    private bool isStunned = false;  // Đang bị choáng (không thể hành động)
    private float stunTimer = 0f;    // Bộ đếm thời gian choáng còn lại

    public bool IsGettingKnockedBack => isGettingKnockedBack;
    public bool IsStunned => isStunned;

    [Header("Knock Back Force")]
    [SerializeField] private float knockbackForceInit = 12f; // Lực đẩy ban đầu

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Xử lý knockback: giảm dần vận tốc theo thời gian
        if (isGettingKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                StopKnockback();
            }
            else
            {
                // Lerp vận tốc từ knockbackVelocity về 0 để tạo hiệu ứng mượt
                float t = 1f - (knockbackTimer / knockbackDuration);
                rb.linearVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, t);
            }
        }

        // Xử lý stun: đếm ngược thời gian choáng
        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (stunTimer <= 0f)
            {
                isStunned = false;
            }
        }
    }

    /// <summary>
    /// Kích hoạt knockback theo hướng và lực chỉ định, đồng thời áp dụng trạng thái choáng (stun).
    /// </summary>
    /// <param name="direction">Hướng đẩy lùi (đã normalized)</param>
    /// <param name="duration">Thời gian diễn ra knockback (giây)</param>
    /// <param name="stunDuration">Tổng thời gian choáng (giây, bao gồm cả thời gian knockback)</param>
    public void PlayKnockback(Vector2 direction, float duration, float stunDuration = 0.5f)
    {
        if (rb == null || duration <= 0f) return;

        isGettingKnockedBack = true;
        knockbackDuration = duration;
        knockbackTimer = duration;
        knockbackVelocity = direction.normalized * knockbackForceInit;

        // Choáng diễn ra đồng thời với knockback
        isStunned = true;
        stunTimer = Mathf.Max(stunDuration, duration); // Đảm bảo thời gian choáng tối thiểu bằng thời gian đẩy lùi

        // Gán vận tốc ban đầu ngay lập tức
        rb.linearVelocity = knockbackVelocity;
    }

    /// <summary>
    /// Kết thúc knockback. Nếu không còn choáng thì dừng hẳn vận tốc.
    /// </summary>
    private void StopKnockback()
    {
        isGettingKnockedBack = false;
        if (rb != null && !isStunned)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}

using System.Collections;
using UnityEngine;

public class KnockbackHandler : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGettingKnockedBack = false;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0f;
    private Vector2 knockbackVelocity;

    private bool isStunned = false;
    private float stunTimer = 0f;

    public bool IsGettingKnockedBack => isGettingKnockedBack;
    public bool IsStunned => isStunned;


    [Header("Knock Back Force")]
    [SerializeField] private float knockbackForceInit = 12f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isGettingKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                StopKnockback();
            }
            else
            {
                // Giảm dần lực đẩy theo thời gian để tạo chuyển động mượt mà
                float t = 1f - (knockbackTimer / knockbackDuration);
                rb.linearVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, t);
            }
        }

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
    /// <param name="force">Độ mạnh của lực đẩy lùi</param>
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

    private void StopKnockback()
    {
        isGettingKnockedBack = false;
        // Nếu vẫn còn bị choáng, ta sẽ đứng im chứ không chuyển động nữa
        if (rb != null && !isStunned)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}

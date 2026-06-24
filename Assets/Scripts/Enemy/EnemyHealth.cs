using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý máu, animation Hurt / Die cho enemy.
/// Gắn component này vào cùng GameObject với EnemyAI.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1.5f;   // Thời gian chờ sau khi die animation chạy xong

    [Header("Flash on Hit")]
    [SerializeField] private bool enableHitFlash = true;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;

    [Header("Stun")]
    [SerializeField] private float defaultStunDuration = 0.5f;   // Tổng thời gian choáng (giây) bao gồm cả knockback



    public bool IsDead { get; private set; } = false;

    private Animator anim;
    private EnemyAI enemyAI;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private KnockbackHandler knockbackHandler;

    private void Awake()
    {
        anim           = GetComponent<Animator>();
        enemyAI        = GetComponent<EnemyAI>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackHandler = GetComponent<KnockbackHandler>();

        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Gây sát thương cho enemy và tác dụng lực đẩy lùi kèm choáng (stun). Gọi từ PlayerCombat khi hitbox chạm.
    /// </summary>
    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackDuration = 0.15f, float stunDuration = -1f)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[EnemyHealth] {name} nhận {damage} sát thương — HP còn: {currentHealth}/{maxHealth}");

        // Áp dụng knockback đẩy lùi và choáng
        if (knockbackHandler != null)
        {
            float actualStun = stunDuration >= 0f ? stunDuration : defaultStunDuration;
            knockbackHandler.PlayKnockback(knockbackDirection, knockbackDuration, actualStun);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayHurt();
        }
    }

    private void PlayHurt()
    {
        if (anim != null)
            anim.SetTrigger("hurt");

        if (enableHitFlash && spriteRenderer != null)
            StartCoroutine(HitFlashRoutine());
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Tắt AI & physics để enemy không còn di chuyển / tấn công
        if (enemyAI != null)
            enemyAI.OnDeath();

        // Phát animation die
        if (anim != null)
            anim.SetTrigger("die");

        // Tắt collider để player không bị chặn
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Xóa GameObject sau khi animation chạy xong
        Destroy(gameObject, destroyDelay);

        Debug.Log($"[EnemyHealth] {name} đã chết.");
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null)               // guard: có thể đã Destroy
            spriteRenderer.color = originalColor;
    }

    public int CurrentHealth => currentHealth;
    public int MaxHealth     => maxHealth;
}

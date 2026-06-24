using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý máu, trạng thái Hurt và Death của Player.
/// Gắn component này vào cùng GameObject với PlayerController.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Death")]
    [SerializeField] private float deathRespawnDelay = 2f;  // Thời gian chờ sau die animation trước khi xử lý respawn

    [Header("Invincibility Frames")]
    [SerializeField] private float invincibilityDuration = 0.8f; // Giây bất tử sau khi nhận damage (tránh bị trúng liên tiếp)
    [SerializeField] private bool enableHurtFlash = true;
    [SerializeField] private float flashInterval = 0.1f;

    public bool IsDead { get; private set; } = false;
    public bool IsInvincible { get; private set; } = false;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // ─── Animator parameters ───────────────────────────────────────────────────
    // Đảm bảo Animator của Player có đúng các trigger/bool này:
    //   Trigger  "hurt"
    //   Trigger  "die"
    // ───────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        animator      = GetComponent<Animator>();
        rb            = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gây sát thương cho Player. Gọi từ EnemyAI khi hitbox tấn công chạm.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead || IsInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[PlayerStats] Player nhận {damage} sát thương — HP còn: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayHurt();
            StartCoroutine(InvincibilityRoutine());
        }
    }

    /// <summary>
    /// Hồi máu cho Player (dùng ở Hub hoặc item).
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[PlayerStats] Player hồi {amount} máu — HP: {currentHealth}/{maxHealth}");
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Internal helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void PlayHurt()
    {
        if (animator != null)
            animator.SetTrigger("hurt");
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Dừng di chuyển
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Tắt input movement (PlayerController sẽ check IsDead)
        if (animator != null)
            animator.SetTrigger("die");

        Debug.Log("[PlayerStats] Player đã chết.");

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathRespawnDelay);

        // TODO: Gọi GameManager để hiện Death Screen hoặc Respawn
        // GameManager.Instance?.OnPlayerDeath();
        Debug.Log("[PlayerStats] Death sequence hoàn tất — chờ GameManager xử lý respawn.");
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (enableHurtFlash && spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        IsInvincible = false;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Properties
    // ──────────────────────────────────────────────────────────────────────────

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}

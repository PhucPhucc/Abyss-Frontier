using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý máu, nhận sát thương, hồi máu và xử lý chết của Player.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int currentHealth;

    [Header("Invincibility Frames")]
    [SerializeField] private float invincibilityDuration = 0.8f;  // Giây bất tử sau khi bị đánh
    [SerializeField] private float flashInterval = 0.1f;           // Tốc độ nhấp nháy sprite

    [Header("Death")]
    [SerializeField] private float deathSequenceDuration = 2f;    // Thời gian chờ sau die animation

    // ─── State ────────────────────────────────────────────────────────────────
    public bool IsDead { get; private set; } = false;
    public bool IsInvincible { get; private set; } = false;

    // ─── References ──────────────────────────────────────────────────────────
    private PlayerStats playerStats;
    private PlayerController playerController;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => playerStats != null ? playerStats.MaxHealth : 70;

    // ─── Animator parameter names ─────────────────────────────────────────────
    // Animator của Player cần có:
    //   Trigger "hurt"  → State Hurt → trả về Idle/Walk
    //   Trigger "die"   → State Die  (không loop, không trả về)
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        playerStats     = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();
        animator        = GetComponent<Animator>();
        spriteRenderer  = GetComponent<SpriteRenderer>();
        rb              = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = MaxHealth;
    }


    /// <summary>
    /// Nhận sát thương từ Enemy. Không gây knockback (Enemy attack).
    /// Có hệ thống né tránh (DodgeChance) và Invincibility Frames.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead || IsInvincible) return;

        // Kiểm tra né tránh dựa trên Dexterity
        if (playerStats != null && UnityEngine.Random.value < playerStats.DodgeChance)
        {
            Debug.Log("[PlayerHealth] Player né tránh thành công!");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"[PlayerHealth] Player nhận {damage} sát thương — HP còn: {currentHealth}/{MaxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Phát animation hurt
            if (animator != null) animator.SetTrigger("hurt");

            // Kích hoạt invincibility frames (nhấp nháy)
            StartCoroutine(InvincibilityRoutine());
        }
    }

    /// <summary>
    /// Hồi một lượng máu nhất định (không vượt quá MaxHealth).
    /// </summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
        Debug.Log($"[PlayerHealth] Hồi {amount} máu — HP: {currentHealth}/{MaxHealth}");
    }

    /// <summary>
    /// Hồi đầy máu. Gọi khi Player nghỉ tại Hub.
    /// </summary>
    public void RestoreFullHealth()
    {
        IsDead = false;
        currentHealth = MaxHealth;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Internal helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // Dừng di chuyển ngay lập tức
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Tắt collider để enemy không tiếp tục tấn công vào xác
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Phát animation die
        if (animator != null) animator.SetTrigger("die");

        Debug.Log("[PlayerHealth] Player đã chết!");

        // Reset EXP (theo game design: mất EXP khi chết)
        if (playerStats != null) playerStats.ResetExpToZero();

        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        yield return new WaitForSeconds(deathSequenceDuration);

        // Hồi phục: bật lại collider, reset HP
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        currentHealth = MaxHealth;
        IsDead = false;

        // TODO: Thay thế bằng GameManager.Instance.OnPlayerDeath() để hiện Death Screen
        Debug.Log("[PlayerHealth] Death sequence hoàn tất — sẵn sàng respawn.");
    }

    private IEnumerator InvincibilityRoutine()
    {
        IsInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Đảm bảo sprite luôn hiện sau khi kết thúc
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        IsInvincible = false;
    }
}

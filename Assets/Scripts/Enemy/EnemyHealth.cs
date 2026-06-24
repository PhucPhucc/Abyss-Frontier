using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý máu, nhận sát thương, hiệu ứng trúng đòn và xử lý chết của Enemy.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Stats Definition")]
    [SerializeField] private EnemyLevel enemyLevel = EnemyLevel.Level1;   // Cấp độ của enemy (1-3)
    [SerializeField] private EnemyStats statsDefinition;                   // ScriptableObject chứa chỉ số gốc

    [Header("Health")]
    [SerializeField] private int maxHealth = 30;       // Máu tối đa (sẽ được ghi đè bởi statsDefinition nếu có)
    [SerializeField] private int currentHealth;         // Máu hiện tại

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1.5f; // Thời gian trước khi xóa object sau khi chết

    [Header("Flash on Hit")]
    [SerializeField] private bool enableHitFlash = true; // Bật/tắt hiệu ứng nhấp nháy khi trúng đòn
    [SerializeField] private float flashDuration = 0.1f; // Thời gian flash
    [SerializeField] private Color flashColor = Color.red; // Màu flash

    [Header("Stun")]
    [SerializeField] private float defaultStunDuration = 0.5f; // Thời gian choáng mặc định khi bị đánh

    private int def;             // Phòng thủ (giảm sát thương)
    private bool isDead = false; // Cờ chết

    private Animator anim;
    private EnemyAI enemyAI;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private KnockbackHandler knockbackHandler;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public EnemyLevel EnemyLevel => enemyLevel;

    private void Awake()
    {
        // Cache các component cần thiết
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackHandler = GetComponent<KnockbackHandler>();

        // Nếu có statsDefinition, lấy chỉ số theo cấp độ
        if (statsDefinition != null)
        {
            int level = (int)enemyLevel;
            maxHealth = statsDefinition.GetHP(level);
            def = statsDefinition.GetDEF(level);

            if (enemyAI != null)
                enemyAI.SetStatsFromDefinition(statsDefinition, level);
        }

        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Nhận sát thương từ Player (hoặc nguồn khác). Có kèm knockback và stun.
    /// </summary>
    /// <param name="damage">Sát thương gốc</param>
    /// <param name="knockbackDirection">Hướng đẩy lùi</param>
    /// <param name="knockbackDuration">Thời gian đẩy lùi</param>
    /// <param name="stunDuration">Thời gian choáng (‑1 = dùng default)</param>
    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackDuration = 0.15f, float stunDuration = -1f)
    {
        if (isDead) return;

        // Tính sát thương thực tế sau khi trừ phòng thủ (tối thiểu 1)
        int actualDamage = Mathf.Max(1, damage - def);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[EnemyHealth] {name} nhận {actualDamage} sát thương — HP còn: {currentHealth}/{maxHealth}");

        // Kích hoạt knockback nếu có component
        if (knockbackHandler != null)
        {
            float actualStun = stunDuration >= 0f ? stunDuration : defaultStunDuration;
            knockbackHandler.PlayKnockback(knockbackDirection, knockbackDuration, actualStun);
        }

        if (currentHealth <= 0)
            Die();
        else
            PlayHurt();
    }

    /// <summary>
    /// Phát hoạt ảnh bị thương và hiệu ứng flash.
    /// </summary>
    private void PlayHurt()
    {
        if (anim != null)
            anim.SetTrigger("hurt");

        if (enableHitFlash && spriteRenderer != null)
            StartCoroutine(HitFlashRoutine());
    }

    /// <summary>
    /// Xử lý khi enemy chết: dừng AI, phát hoạt ảnh chết, tắt collider, cộng EXP cho Player.
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Báo cho AI biết để dừng mọi hành vi
        if (enemyAI != null)
            enemyAI.OnDeath();

        if (anim != null)
            anim.SetTrigger("die");

        // Tắt collider để không bị tương tác thêm
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        GrantExpToPlayer();

        Destroy(gameObject, destroyDelay);
        Debug.Log($"[EnemyHealth] {name} đã chết.");
    }

    /// <summary>
    /// Trao thưởng EXP cho Player dựa trên statsDefinition và cấp độ.
    /// </summary>
    private void GrantExpToPlayer()
    {
        if (statsDefinition == null) return;

        int expReward = statsDefinition.GetExpReward((int)enemyLevel);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerStats ps = player.GetComponent<PlayerStats>();
            if (ps != null)
                ps.AddExp(expReward);
        }
    }

    /// <summary>
    /// Coroutine nhấp nháy màu đỏ khi trúng đòn.
    /// </summary>
    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}

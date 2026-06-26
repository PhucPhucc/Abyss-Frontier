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
    private bool hasMoveParams;

    public event System.Action<int, int> HealthChanged;
    public event System.Action Died;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public EnemyLevel EnemyLevel => enemyLevel;
    public float HealthFraction => maxHealth <= 0 ? 0f : currentHealth / (float)maxHealth;

    private void Awake()
    {
        // Cache các component cần thiết
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackHandler = GetComponent<KnockbackHandler>();

        if (anim != null)
        {
            foreach (var param in anim.parameters)
            {
                if (param.name == "lastMoveX")
                {
                    hasMoveParams = true;
                    break;
                }
            }
        }

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
        NotifyHealthChanged();

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
        NotifyHealthChanged();

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
            PlayHurt(knockbackDirection);
    }

    private void PlayHurt(Vector2 knockbackDirection)
    {
        AudioManager.Instance?.PlayEnemyHurt();

        if (anim != null)
        {
         
            if (knockbackDirection != Vector2.zero && hasMoveParams)
            {
                anim.SetFloat("lastMoveX", knockbackDirection.normalized.x * -1);
                anim.SetFloat("lastMoveY", knockbackDirection.normalized.y * -1);
            }
            anim.SetTrigger("hurt");
        }

        if (enableHitFlash && spriteRenderer != null)
            StartCoroutine(HitFlashRoutine());
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (enemyAI != null)
            enemyAI.OnDeath();

        if (anim != null)
            anim.SetTrigger("die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        DropExpOrbs();
        // GrantExpToPlayer();
        Died?.Invoke();

        Destroy(gameObject, destroyDelay);
        Debug.Log($"[EnemyHealth] {name} đã chết.");
    }

    private void DropExpOrbs()
    {
        if (statsDefinition == null) return;

        int expReward = statsDefinition.GetExpReward((int)enemyLevel);
        ExpDropSpawner.Spawn(transform.position, expReward);
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

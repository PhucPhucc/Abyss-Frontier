using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats Definition")]
    [SerializeField] private EnemyLevel enemyLevel = EnemyLevel.Level1;
    [SerializeField] private EnemyStats statsDefinition;

    [Header("Health")]
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int currentHealth;

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1.5f;
    [Header("Flash on Hit")]
    [SerializeField] private bool enableHitFlash = true;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.red;
    [Header("Stun")]
    [SerializeField] private float defaultStunDuration = 0.5f;

    private int def;
    private bool isDead = false;

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
        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackHandler = GetComponent<KnockbackHandler>();

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

    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackDuration = 0.15f, float stunDuration = -1f)
    {
        if (isDead) return;

        int actualDamage = Mathf.Max(1, damage - def);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[EnemyHealth] {name} nhận {actualDamage} sát thương — HP còn: {currentHealth}/{maxHealth}");

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

    private void PlayHurt()
    {
        if (anim != null)
            anim.SetTrigger("hurt");

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

        GrantExpToPlayer();

        Destroy(gameObject, destroyDelay);
        Debug.Log($"[EnemyHealth] {name} đã chết.");
    }

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

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}

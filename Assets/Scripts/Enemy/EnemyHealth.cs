using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý máu, nhận sát thương, hiệu ứng trúng đòn và xử lý chết của Enemy.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    private bool IsMultiplayerServer => GameSessionData.IsMultiplayer && GameSessionData.IsHost;
    public static HashSet<string> KilledEnemyIds { get; } = new HashSet<string>();
    [Header("Stats Definition")]
    [SerializeField] private EnemyLevel enemyLevel = EnemyLevel.Level1;   // Cấp độ của enemy (1-3)
    [SerializeField] private EnemyStats statsDefinition;                   // ScriptableObject chứa chỉ số gốc

    [Header("Save")]
    [SerializeField] private string saveId;             // ID duy nhất để lưu trạng thái (cần set trên prefab/scene instance)

    [Header("Health")]
    [SerializeField] private int maxHealth = 30;       // Máu tối đa (sẽ được ghi đè bởi statsDefinition nếu có)
    [SerializeField] private int currentHealth;         // Máu hiện tại

    [Header("Death")]
    [SerializeField] private float destroyDelay = 1.5f; // Thời gian trước khi xóa object sau khi chết

    [Header("Respawn")]
    [Tooltip("If true, queue an inactive clone and revive it when the player rests at Base Camp. Bosses should leave this off.")]
    [SerializeField] private bool respawnOnDeath = true;
    [Tooltip("If true, respawn inside the dungeon after Respawn Delay. Otherwise, revive when the player rests at Base Camp.")]
    [SerializeField] private bool respawnInDungeonAfterDelay;
    [Tooltip("Delay used only when dungeon timed respawn is enabled.")]
    [SerializeField, Min(0f)] private float respawnDelay = 5f;
    [SerializeField] private bool respawnAtInitialPosition = true;

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
    private Vector3 initialRespawnPosition;
    private Quaternion initialRespawnRotation;
    private Transform initialRespawnParent;

    public event System.Action<int, int> HealthChanged;
    public event System.Action Died;

    public string SaveId => saveId;
    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;

    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        isDead = currentHealth <= 0;
        NotifyHealthChanged();
    }
    public int MaxHealth => maxHealth;
    public EnemyLevel EnemyLevel => enemyLevel;
    public float HealthFraction => maxHealth <= 0 ? 0f : currentHealth / (float)maxHealth;

    private void Awake()
    {
        if (string.IsNullOrEmpty(saveId))
        {
            saveId = $"enemy_{gameObject.scene.name}_{gameObject.name}_{GetSiblingPath(gameObject.transform)}";
        }

        anim = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        knockbackHandler = GetComponent<KnockbackHandler>();
        initialRespawnPosition = transform.position;
        initialRespawnRotation = transform.rotation;
        initialRespawnParent = transform.parent;

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

        if (statsDefinition != null)
        {
            int level = (int)enemyLevel;
            maxHealth = statsDefinition.GetHP(level);
            def = statsDefinition.GetDEF(level);

            if (enemyAI != null)
                enemyAI.SetStatsFromDefinition(statsDefinition, level);
        }

        currentHealth = maxHealth;
        isDead = false;
        if (!string.IsNullOrEmpty(saveId))
            KilledEnemyIds.Remove(saveId);
        NotifyHealthChanged();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(saveId))
            KilledEnemyIds.Remove(saveId);
    }

    /// <summary>
    /// Nhận sát thương từ Player (hoặc nguồn khác). Có kèm knockback và stun.
    /// </summary>
    /// <param name="damage">Sát thương gốc</param>
    /// <param name="knockbackDirection">Hướng đẩy lùi</param>
    /// <param name="knockbackDuration">Thời gian đẩy lùi</param>
    /// <param name="stunDuration">Thời gian choáng (‑1 = dùng default)</param>
    public void TakeDamage(int damage, Vector2 knockbackDirection, float knockbackDuration = 0.15f, float stunDuration = -1f, Transform source = null)
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

        // Khi bị tấn công, báo cho EnemyAI để chuyển sang trạng thái Chase
        if (source != null && enemyAI != null)
            enemyAI.OnHit(source);

        if (currentHealth <= 0)
            Die();
        else
        {
            PlayHurt(knockbackDirection);
            BroadcastHurtToClients();
        }
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

        // Clone WHILE still alive so hub-respawn copy keeps collider / AI / isDead=false.
        // Instantiating after death mutations copies a disabled corpse that cannot be attacked.
        if (!IsMultiplayerServer)
            ScheduleRespawn();

        isDead = true;

        if (!string.IsNullOrEmpty(saveId))
            KilledEnemyIds.Add(saveId);

        if (enemyAI != null)
            enemyAI.OnDeath();

        if (anim != null)
            anim.SetTrigger("die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        DropExpOrbs();
        Died?.Invoke();

        BroadcastDeathToClients();

        if (IsMultiplayerServer)
            StartCoroutine(DestroyAfterDelay());
        else
            Destroy(gameObject, destroyDelay);

        Debug.Log($"[EnemyHealth] {name} đã chết.");
    }

    /// <summary>
    /// Restore a hub-queued clone so it can be hit and run AI again.
    /// </summary>
    public void RestoreLivingState()
    {
        isDead = false;
        currentHealth = maxHealth;

        foreach (Collider2D collider in GetComponents<Collider2D>())
            collider.enabled = true;

        if (enemyAI == null)
            enemyAI = GetComponent<EnemyAI>();
        enemyAI?.RestoreLivingState();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        NotifyHealthChanged();
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        if (gameObject != null)
            Destroy(gameObject);
    }

    private void BroadcastHurtToClients()
    {
        if (!GameSessionData.IsMultiplayer) return;
        var netEnemy = GetComponent<NetworkEnemy>();
        if (netEnemy != null)
            netEnemy.RPC_PlayHurt();
    }

    private void BroadcastDeathToClients()
    {
        if (!GameSessionData.IsMultiplayer) return;
        var netEnemy = GetComponent<NetworkEnemy>();
        if (netEnemy != null)
            netEnemy.RPC_BroadcastDie();
    }

    private void ScheduleRespawn()
    {
        if (!respawnOnDeath)
            return;

        Vector3 respawnPosition = respawnAtInitialPosition ? initialRespawnPosition : transform.position;
        Quaternion respawnRotation = respawnAtInitialPosition ? initialRespawnRotation : transform.rotation;
        Transform respawnParent = initialRespawnParent != null ? initialRespawnParent : transform.parent;

        GameObject respawnClone = Instantiate(gameObject, respawnPosition, respawnRotation, respawnParent);
        respawnClone.name = gameObject.name;
        PrepareRespawnClone(respawnClone);
        respawnClone.SetActive(false);

        if (respawnInDungeonAfterDelay)
        {
            EnemyRespawnRunner.ScheduleTimedRespawn(respawnClone, respawnDelay);
            Debug.Log($"[EnemyHealth] {name} will respawn in {respawnDelay:0.##} seconds.");
            return;
        }

        EnemyRespawnRunner.RegisterForHubRespawn(respawnClone);
        Debug.Log($"[EnemyHealth] {name} queued for hub respawn at Base Camp.");
    }

    private void PrepareRespawnClone(GameObject respawnClone)
    {
        Rigidbody2D cloneRigidbody = respawnClone.GetComponent<Rigidbody2D>();
        if (cloneRigidbody != null)
        {
            cloneRigidbody.linearVelocity = Vector2.zero;
            cloneRigidbody.angularVelocity = 0f;
        }

        Animator cloneAnimator = respawnClone.GetComponent<Animator>();
        if (cloneAnimator != null)
        {
            cloneAnimator.Rebind();
            cloneAnimator.Update(0f);
        }

        EnemyHealth cloneHealth = respawnClone.GetComponent<EnemyHealth>();
        cloneHealth?.RestoreLivingState();
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

    private static string GetSiblingPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null && t.parent != t.root)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}

/// <summary>
/// Holds inactive enemy clones and revives them only when the player rests at Base Camp.
/// </summary>
public sealed class EnemyRespawnRunner : MonoBehaviour
{
    private static EnemyRespawnRunner instance;
    private static readonly List<GameObject> pendingHubRespawns = new List<GameObject>();

    public static void RegisterForHubRespawn(GameObject enemyToRespawn)
    {
        if (enemyToRespawn == null)
            return;

        EnsureInstance();

        if (!pendingHubRespawns.Contains(enemyToRespawn))
            pendingHubRespawns.Add(enemyToRespawn);
    }

    public static void RespawnAllAtHub()
    {
        for (int i = 0; i < pendingHubRespawns.Count; i++)
        {
            GameObject enemy = pendingHubRespawns[i];
            if (enemy == null)
                continue;

            // Fix clones queued before the death-order bug (disabled collider / dead AI).
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            health?.RestoreLivingState();

            enemy.SetActive(true);
        }

        pendingHubRespawns.Clear();
    }

    public static void ScheduleTimedRespawn(GameObject enemyToRespawn, float delay)
    {
        if (enemyToRespawn == null)
            return;

        EnsureInstance();
        instance.StartCoroutine(instance.RespawnAfterDelay(enemyToRespawn, Mathf.Max(0f, delay)));
    }

    private IEnumerator RespawnAfterDelay(GameObject enemyToRespawn, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);
        else
            yield return null;

        if (enemyToRespawn != null)
            enemyToRespawn.SetActive(true);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        GameObject runnerObject = new GameObject("Enemy Respawn Runner");
        instance = runnerObject.AddComponent<EnemyRespawnRunner>();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}

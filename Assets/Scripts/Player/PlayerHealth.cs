using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth;

    private PlayerStats playerStats;
    private NetworkPlayer networkPlayer;
    private CharacterAnimationHandler animHandler;
    private bool isDead;
    private bool isInvulnerable;
    private Coroutine invulnerabilityFlashRoutine;
    private Coroutine deathSequenceRoutine;

    public event System.Action<int, int> HealthChanged;
    public event System.Action Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => playerStats != null ? playerStats.MaxHealth : 70;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;
    public float HealthFraction => MaxHealth <= 0 ? 0f : currentHealth / (float)MaxHealth;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        networkPlayer = GetComponent<NetworkPlayer>() ?? GetComponentInParent<NetworkPlayer>();
        animHandler = GetComponent<CharacterAnimationHandler>();
        ResetHealthState();
    }

    private void Start()
    {
        ResetHealthState();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0 || isInvulnerable) return;

        if (playerStats != null && Random.value < playerStats.DodgeChance)
        {
            Debug.Log("Player dodged the attack!");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        NotifyHealthChanged();

        AudioManager.Instance?.PlayPlayerHurt();
        animHandler?.TriggerHurt();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player has died.");
        AudioManager.Instance?.PlayPlayerDeath();
        if (playerStats != null)
            playerStats.ResetExpToZero();

        if (deathSequenceRoutine != null)
            StopCoroutine(deathSequenceRoutine);
        deathSequenceRoutine = StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        animHandler?.TriggerDeath();

        if (ShouldShowLocalDeathUI())
            Died?.Invoke();

        if (animHandler != null)
            yield return animHandler.WaitForDeathAnimationRoutine();
        else
            yield return new WaitForSeconds(1f);

        if (ShouldShowLocalDeathUI())
            DeathScreenUI.ShowDeath(this);
        deathSequenceRoutine = null;
    }

    public void Respawn()
    {
        if (deathSequenceRoutine != null)
        {
            StopCoroutine(deathSequenceRoutine);
            deathSequenceRoutine = null;
        }

        isDead = false;
        SetInvulnerable(false);
        currentHealth = MaxHealth;
        NotifyHealthChanged();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        animHandler?.TriggerRespawn();

        PlayerStats stats = GetComponent<PlayerStats>();
        stats?.RestoreVitals();
    }

    public void SetInvulnerable(bool value)
    {
        if (isInvulnerable == value)
            return;

        isInvulnerable = value;

        if (invulnerabilityFlashRoutine != null)
        {
            StopCoroutine(invulnerabilityFlashRoutine);
            invulnerabilityFlashRoutine = null;
        }

        if (isInvulnerable)
            invulnerabilityFlashRoutine = StartCoroutine(InvulnerabilityFlashRoutine());
        else
            ResetSpriteAlpha();
    }

    private IEnumerator InvulnerabilityFlashRoutine()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float flashInterval = 0.06f;

        while (isInvulnerable)
        {
            float alpha = Mathf.PingPong(Time.time / flashInterval, 1f) > 0.5f ? 0.45f : 1f;
            SetSpriteAlpha(renderers, alpha);
            yield return null;
        }

        ResetSpriteAlpha();
        invulnerabilityFlashRoutine = null;
    }

    private static void SetSpriteAlpha(SpriteRenderer[] renderers, float alpha)
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null) continue;
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    private void ResetSpriteAlpha()
    {
        SetSpriteAlpha(GetComponentsInChildren<SpriteRenderer>(), 1f);
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        int newHealth = Mathf.Min(currentHealth + amount, MaxHealth);
        if (newHealth == currentHealth) return;

        currentHealth = newHealth;
        NotifyHealthChanged();
    }

    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        isDead = currentHealth <= 0;
        NotifyHealthChanged();
    }

    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;
        isDead = false;
        NotifyHealthChanged();
    }

    private void ResetHealthState()
    {
        currentHealth = MaxHealth;
        isDead = false;
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    private bool ShouldShowLocalDeathUI()
    {
        if (!GameSessionData.IsMultiplayer)
            return true;

        return networkPlayer == null || (networkPlayer.Object != null && networkPlayer.Object.HasInputAuthority);
    }
}
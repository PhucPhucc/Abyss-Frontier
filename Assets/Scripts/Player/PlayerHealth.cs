using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth;

    private PlayerStats playerStats;
    private CharacterAnimationHandler animHandler;
    private bool isDead;

    public event System.Action<int, int> HealthChanged;
    public event System.Action Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => playerStats != null ? playerStats.MaxHealth : 70;
    public bool IsDead => isDead;
    public float HealthFraction => MaxHealth <= 0 ? 0f : currentHealth / (float)MaxHealth;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        animHandler = GetComponent<CharacterAnimationHandler>();
        ResetHealthState();
    }

    private void Start()
    {
        ResetHealthState();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0) return;

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
        animHandler?.TriggerDeath();
        if (playerStats != null)
        {
            playerStats.ResetExpToZero();
        }

        Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        int newHealth = Mathf.Min(currentHealth + amount, MaxHealth);
        if (newHealth == currentHealth) return;

        currentHealth = newHealth;
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
}

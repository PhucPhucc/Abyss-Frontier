using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth;

    private PlayerStats playerStats;
    private CharacterAnimationHandler animHandler;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => playerStats != null ? playerStats.MaxHealth : 70;
    public bool IsDead => isDead;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        animHandler = GetComponent<CharacterAnimationHandler>();
    }

    private void Start()
    {
        currentHealth = MaxHealth;
        isDead = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (playerStats != null && Random.value < playerStats.DodgeChance)
        {
            Debug.Log("Player dodged the attack!");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

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
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
    }

    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;
        isDead = false;
    }
}
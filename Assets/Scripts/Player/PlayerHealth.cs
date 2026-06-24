using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth;

    private PlayerStats playerStats;
    private PlayerController playerController;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => playerStats != null ? playerStats.MaxHealth : 70;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentHealth = MaxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (playerStats != null && Random.value < playerStats.DodgeChance)
        {
            Debug.Log("Player dodged the attack!");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (playerController != null)
        {
            playerController.TriggerHurt();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        if (playerStats != null)
        {
            playerStats.ResetExpToZero();
        }
        currentHealth = MaxHealth;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
    }

    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;
    }
}

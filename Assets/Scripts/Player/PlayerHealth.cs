using UnityEngine;

/// <summary>
/// Quản lý máu, nhận sát thương, hồi máu và xử lý chết của Player.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int currentHealth; // Máu hiện tại

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

    /// <summary>
    /// Nhận sát thương. Nếu Player có né tránh (DodgeChance) thì có thể né đòn.
    /// Khi máu về 0 thì chết.
    /// </summary>
    public void TakeDamage(int damage)
    {
        // Kiểm tra né tránh dựa trên Dexterity
        if (playerStats != null && Random.value < playerStats.DodgeChance)
        {
            Debug.Log("Player dodged the attack!");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Kích hoạt animation bị thương
        if (playerController != null)
        {
            playerController.TriggerHurt();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Xử lý khi Player chết: reset EXP về 0, hồi đầy máu.
    /// </summary>
    private void Die()
    {
        Debug.Log("Player has died.");
        if (playerStats != null)
        {
            playerStats.ResetExpToZero();
        }
        currentHealth = MaxHealth;
    }

    /// <summary>
    /// Hồi một lượng máu nhất định (không vượt quá MaxHealth).
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
    }

    /// <summary>
    /// Hồi đầy máu. Gọi khi Player nghỉ tại Hub.
    /// </summary>
    public void RestoreFullHealth()
    {
        currentHealth = MaxHealth;
    }
}

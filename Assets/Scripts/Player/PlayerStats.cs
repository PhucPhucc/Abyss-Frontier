using System;
using UnityEngine;

public enum PlayerStatType
{
    MaxHealth,
    Attack,
    Defense,
    MaxStamina
}

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int defense = 5;

    [Header("Progression")]
    [SerializeField] private int experiencePerStatPoint = 100;
    [SerializeField] private int healthUpgradeAmount = 10;
    [SerializeField] private float staminaUpgradeAmount = 10f;
    [SerializeField] private int attackUpgradeAmount = 2;
    [SerializeField] private int defenseUpgradeAmount = 1;

    private int currentHealth;
    private float currentStamina;
    private int currentExperience;
    private int statPoints;

    public event Action StatsChanged;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public int AttackDamage => attackDamage;
    public int Defense => defense;
    public int CurrentExperience => currentExperience;
    public int ExperiencePerStatPoint => experiencePerStatPoint;
    public int StatPoints => statPoints;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        maxStamina = Mathf.Max(1f, maxStamina);
        attackDamage = Mathf.Max(0, attackDamage);
        defense = Mathf.Max(0, defense);
        experiencePerStatPoint = Mathf.Max(1, experiencePerStatPoint);

        currentHealth = currentHealth > 0 ? Mathf.Clamp(currentHealth, 0, maxHealth) : maxHealth;
        currentStamina = currentStamina > 0f ? Mathf.Clamp(currentStamina, 0f, maxStamina) : maxStamina;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentExperience += amount;
        while (currentExperience >= experiencePerStatPoint)
        {
            currentExperience -= experiencePerStatPoint;
            statPoints++;
        }

        NotifyChanged();
    }

    public bool TryUpgradeStat(PlayerStatType statType)
    {
        if (statPoints <= 0)
        {
            return false;
        }

        switch (statType)
        {
            case PlayerStatType.MaxHealth:
                maxHealth += healthUpgradeAmount;
                currentHealth = Mathf.Min(maxHealth, currentHealth + healthUpgradeAmount);
                break;
            case PlayerStatType.Attack:
                attackDamage += attackUpgradeAmount;
                break;
            case PlayerStatType.Defense:
                defense += defenseUpgradeAmount;
                break;
            case PlayerStatType.MaxStamina:
                maxStamina += staminaUpgradeAmount;
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaUpgradeAmount);
                break;
            default:
                return false;
        }

        statPoints--;
        NotifyChanged();
        return true;
    }

    public void RestoreVitals()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        NotifyChanged();
    }

    public void ResetExperienceOnDeath()
    {
        currentExperience = 0;
        statPoints = 0;
        NotifyChanged();
    }

    public void TakeDamage(int rawDamage)
    {
        int mitigatedDamage = Mathf.Max(0, rawDamage - defense);
        currentHealth = Mathf.Max(0, currentHealth - mitigatedDamage);
        NotifyChanged();
    }

    public bool SpendStamina(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (currentStamina < amount)
        {
            return false;
        }

        currentStamina -= amount;
        NotifyChanged();
        return true;
    }

    public void RecoverStamina(float amount)
    {
        if (amount <= 0f || currentStamina >= maxStamina)
        {
            return;
        }

        float newStamina = Mathf.Min(maxStamina, currentStamina + amount);
        if (!Mathf.Approximately(newStamina, currentStamina))
        {
            currentStamina = newStamina;
            NotifyChanged();
        }
    }

    private void NotifyChanged()
    {
        StatsChanged?.Invoke();
    }
}

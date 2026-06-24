using UnityEngine;

public enum StatType
{
    Strength,
    Dexterity,
    Vitality,
    Agility,
    Endurance,
    Intelligence
}

public class PlayerStats : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int expToNextLevel = 100;
    [SerializeField] private int statPointsPerLevel = 5;

    [Header("Base Stats")]
    [SerializeField] private int strength = 1;
    [SerializeField] private int dexterity = 1;
    [SerializeField] private int vitality = 1;
    [SerializeField] private int agility = 1;
    [SerializeField] private int endurance = 1;
    [SerializeField] private int intelligence = 1;

    [Header("Stat Points")]
    [SerializeField] private int availableStatPoints = 5;

    [Header("Derived Stats (Read Only)")]
    [SerializeField] private int derivedMaxHealth = 70;
    [SerializeField] private int derivedAttackDamage = 7;
    [SerializeField] private float derivedDodgeChance = 0.02f;
    [SerializeField] private float derivedMoveSpeed = 2.65f;
    [SerializeField] private float derivedStaminaEfficiency = 1.1f;
    [SerializeField] private float derivedExpMultiplier = 1.1f;

    private const int BASE_HP = 50;
    private const int HP_PER_VIT = 20;
    private const int BASE_ATK = 5;
    private const int ATK_PER_STR = 2;
    private const float DODGE_PER_DEX = 0.02f;
    private const float BASE_SPEED = 2.5f;
    private const float SPEED_PER_AGI = 0.15f;
    private const float ENDURANCE_FACTOR = 0.1f;
    private const float EXP_PER_INT = 0.1f;

    private PlayerHealth _playerHealth;

    public bool IsDead => _playerHealth != null && _playerHealth.CurrentHealth <= 0;

    public void TakeDamage(int damage)
    {
        if (_playerHealth == null)
            _playerHealth = GetComponent<PlayerHealth>();
        _playerHealth?.TakeDamage(damage);
    }

    public int Level => level;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => expToNextLevel;
    public int AvailableStatPoints => availableStatPoints;

    public int Strength => strength;
    public int Dexterity => dexterity;
    public int Vitality => vitality;
    public int Agility => agility;
    public int Endurance => endurance;
    public int Intelligence => intelligence;

    public int MaxHealth => derivedMaxHealth;
    public int AttackDamage => derivedAttackDamage;
    public float DodgeChance => derivedDodgeChance;
    public float MoveSpeed => derivedMoveSpeed;
    public float StaminaEfficiency => derivedStaminaEfficiency;
    public float ExpMultiplier => derivedExpMultiplier;

    private void Awake()
    {
        RecalculateDerivedStats();
    }

    public void RecalculateDerivedStats()
    {
        derivedMaxHealth = BASE_HP + (vitality * HP_PER_VIT);
        derivedAttackDamage = BASE_ATK + (strength * ATK_PER_STR);
        derivedDodgeChance = Mathf.Min(dexterity * DODGE_PER_DEX, 0.5f);
        derivedMoveSpeed = BASE_SPEED + (agility * SPEED_PER_AGI);
        derivedStaminaEfficiency = 1f + (endurance * ENDURANCE_FACTOR);
        derivedExpMultiplier = 1f + (intelligence * EXP_PER_INT);
    }

    public void AddExp(int amount)
    {
        int finalExp = Mathf.RoundToInt(amount * derivedExpMultiplier);
        currentExp += finalExp;

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f);
        availableStatPoints += statPointsPerLevel;
        Debug.Log($"Level up! Now level {level}. Stat points: {availableStatPoints}");
    }

    public bool AllocateStat(StatType statType)
    {
        if (availableStatPoints <= 0) return false;

        switch (statType)
        {
            case StatType.Strength: strength++; break;
            case StatType.Dexterity: dexterity++; break;
            case StatType.Vitality: vitality++; break;
            case StatType.Agility: agility++; break;
            case StatType.Endurance: endurance++; break;
            case StatType.Intelligence: intelligence++; break;
            default: return false;
        }

        availableStatPoints--;
        RecalculateDerivedStats();
        return true;
    }

    public void ResetExpToZero()
    {
        currentExp = 0;
    }
}

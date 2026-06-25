using UnityEngine;

/// <summary>
/// Các loại chỉ số (stat) mà Player có thể nâng cấp.
/// </summary>
public enum StatType
{
    Strength,     // Sức mạnh → ATK
    Dexterity,    // Khéo léo → Né tránh
    Vitality,     // Sinh lực → Máu
    Agility,      // Nhanh nhẹn → Tốc độ di chuyển
    Endurance,    // Bền bỉ → Hiệu suất stamina
    Intelligence  // Trí lực → Hệ số EXP
}

/// <summary>
/// Quản lý cấp độ, EXP, chỉ số cơ bản và chỉ số dẫn xuất của Player.
/// Chỉ số dẫn xuất được tính lại mỗi khi có thay đổi.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int level = 1;                   // Cấp độ hiện tại
    [SerializeField] private int currentExp = 0;              // EXP hiện có
    [SerializeField] private int expToNextLevel = 100;        // EXP cần để lên cấp tiếp theo
    [SerializeField] private int statPointsPerLevel = 5;      // Số điểm chỉ số nhận được mỗi cấp

    [Header("Base Stats")]
    [SerializeField] private int strength = 1;     // Sức mạnh
    [SerializeField] private int dexterity = 1;    // Khéo léo
    [SerializeField] private int vitality = 1;     // Sinh lực
    [SerializeField] private int agility = 1;      // Nhanh nhẹn
    [SerializeField] private int endurance = 1;    // Bền bỉ
    [SerializeField] private int intelligence = 1; // Trí lực

    [Header("Stat Points")]
    [SerializeField] private int availableStatPoints = 5; // Điểm chỉ số có thể phân bổ

    [Header("Stamina Fallback")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;

    [Header("Derived Stats (Read Only)")]
    [SerializeField] private int derivedMaxHealth = 70;           // Máu tối đa
    [SerializeField] private int derivedAttackDamage = 7;         // Sát thương
    [SerializeField] private float derivedDodgeChance = 0.02f;     // Tỷ lệ né tránh
    [SerializeField] private float derivedMoveSpeed = 2.65f;      // Tốc độ di chuyển
    [SerializeField] private float derivedStaminaEfficiency = 1.1f; // Hiệu suất stamina
    [SerializeField] private float derivedExpMultiplier = 1.1f;    // Hệ số EXP

    // Hằng số công thức tính chỉ số dẫn xuất
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
    private PlayerController _playerController;

    public event System.Action StatsChanged;

    public bool IsDead => _playerHealth != null && _playerHealth.CurrentHealth <= 0;

    /// <summary>Cầu nối để PlayerHealth nhận sát thương.</summary>
    public void TakeDamage(int damage)
    {
        if (_playerHealth == null)
            _playerHealth = GetComponent<PlayerHealth>();
        _playerHealth?.TakeDamage(damage);
        NotifyChanged();
    }

    // --- Public Properties ---
    public int Level => level;
    public int CurrentExp => currentExp;
    public int CurrentExperience => currentExp;
    public int ExpToNextLevel => expToNextLevel;
    public int ExperiencePerStatPoint => expToNextLevel;
    public int AvailableStatPoints => availableStatPoints;
    public int StatPoints => availableStatPoints;

    public int Strength => strength;
    public int Dexterity => dexterity;
    public int Vitality => vitality;
    public int Agility => agility;
    public int Endurance => endurance;
    public int Intelligence => intelligence;

    public int MaxHealth => derivedMaxHealth;
    public int CurrentHealth
    {
        get
        {
            EnsurePlayerHealth();
            return _playerHealth != null ? _playerHealth.CurrentHealth : derivedMaxHealth;
        }
    }

    public float MaxStamina
    {
        get
        {
            EnsurePlayerController();
            return _playerController != null ? _playerController.MaxStamina : maxStamina;
        }
    }

    public float CurrentStamina
    {
        get
        {
            EnsurePlayerController();
            return _playerController != null ? _playerController.CurrentStamina : currentStamina;
        }
    }

    public int AttackDamage => derivedAttackDamage;
    public int Defense => 0;
    public float DodgeChance => derivedDodgeChance;
    public float MoveSpeed => derivedMoveSpeed;
    public float StaminaEfficiency => derivedStaminaEfficiency;
    public float ExpMultiplier => derivedExpMultiplier;

    private void Awake()
    {
        // Tính chỉ số dẫn xuất ngay khi khởi tạo
        RecalculateDerivedStats();
        currentStamina = maxStamina;
        _playerHealth = GetComponent<PlayerHealth>();
        _playerController = GetComponent<PlayerController>();
    }

    /// <summary>
    /// Tính lại tất cả chỉ số dẫn xuất dựa trên chỉ số cơ bản hiện tại.
    /// Gọi sau mỗi lần phân bổ stat point.
    /// </summary>
    public void RecalculateDerivedStats()
    {
        derivedMaxHealth = BASE_HP + (vitality * HP_PER_VIT);
        derivedAttackDamage = BASE_ATK + (strength * ATK_PER_STR);
        derivedDodgeChance = Mathf.Min(dexterity * DODGE_PER_DEX, 0.5f); // Giới hạn 50%
        derivedMoveSpeed = BASE_SPEED + (agility * SPEED_PER_AGI);
        derivedStaminaEfficiency = 1f + (endurance * ENDURANCE_FACTOR);
        derivedExpMultiplier = 1f + (intelligence * EXP_PER_INT);
    }

    /// <summary>
    /// Cộng EXP (đã nhân hệ số). Nếu đủ EXP để lên cấp, tự động level up.
    /// </summary>
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        int finalExp = Mathf.RoundToInt(amount * derivedExpMultiplier);
        currentExp += finalExp;

        // Có thể lên nhiều cấp cùng lúc nếu nhận nhiều EXP
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }

        NotifyChanged();
    }

    public void AddExperience(int amount) => AddExp(amount);

    /// <summary>
    /// Lên cấp: tăng level, yêu cầu EXP cho cấp sau, cộng điểm stat.
    /// </summary>
    private void LevelUp()
    {
        level++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f);
        availableStatPoints += statPointsPerLevel;
        Debug.Log($"Level up! Now level {level}. Stat points: {availableStatPoints}");
    }

    /// <summary>
    /// Phân bổ một điểm chỉ số vào stat tương ứng.
    /// </summary>
    /// <returns>True nếu phân bổ thành công, false nếu không đủ điểm.</returns>
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
        NotifyChanged();
        return true;
    }

    public void RestoreVitals()
    {
        EnsurePlayerHealth();
        _playerHealth?.RestoreFullHealth();

        EnsurePlayerController();
        if (_playerController != null)
        {
            _playerController.RestoreStamina();
        }
        else
        {
            currentStamina = maxStamina;
        }

        NotifyChanged();
    }

    public bool SpendStamina(float amount)
    {
        if (amount <= 0f) return true;

        EnsurePlayerController();
        if (_playerController != null)
        {
            bool spent = _playerController.SpendStamina(amount);
            if (spent)
            {
                NotifyChanged();
            }

            return spent;
        }

        if (currentStamina < amount) return false;

        currentStamina -= amount;
        NotifyChanged();
        return true;
    }

    public void RecoverStamina(float amount)
    {
        if (amount <= 0f) return;

        EnsurePlayerController();
        if (_playerController != null)
        {
            _playerController.RecoverStamina(amount);
            NotifyChanged();
            return;
        }

        float newStamina = Mathf.Min(maxStamina, currentStamina + amount);
        if (!Mathf.Approximately(newStamina, currentStamina))
        {
            currentStamina = newStamina;
            NotifyChanged();
        }
    }

    /// <summary>
    /// Reset EXP về 0 (gọi khi Player chết).
    /// </summary>
    public void ResetExpToZero()
    {
        currentExp = 0;
        NotifyChanged();
    }

    private void EnsurePlayerHealth()
    {
        if (_playerHealth == null)
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }
    }

    private void EnsurePlayerController()
    {
        if (_playerController == null)
        {
            _playerController = GetComponent<PlayerController>();
        }
    }

    private void NotifyChanged()
    {
        StatsChanged?.Invoke();
    }
}

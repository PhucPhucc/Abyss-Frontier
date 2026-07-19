using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharacterMotor
{
    [Header("Sprint")]
    [SerializeField] private float sprintSpeed = 6f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;

    private PlayerStats playerStats;
    private PlayerCombat cachedCombat;
    private PlayerDash playerDash;
    private float currentStamina;
    private bool isSprintInputPressed;
    private bool isSprinting;

    public void SetSprintInput(bool pressed) => isSprintInputPressed = pressed;

    public bool IsSprinting => isSprinting;
    public bool IsDashing => playerDash != null && playerDash.IsDashing;
    public float CurrentStamina => playerStats != null ? playerStats.CurrentStamina : currentStamina;
    public float MaxStamina => playerStats != null ? playerStats.MaxStamina : maxStamina;

    // Flag: true khi Fusion (multiplayer) đang điều khiển di chuyển qua FixedUpdateNetwork.
    // Khi đó, CharacterMotor.FixedUpdate sẽ bỏ qua để tránh race condition.
    public bool IsControlledByNetwork { get; set; }

    protected override void Awake()
    {
        base.Awake();
        playerStats = GetComponent<PlayerStats>();
        cachedCombat = GetComponent<PlayerCombat>();
        playerDash = GetComponent<PlayerDash>();
        if (playerDash == null)
            playerDash = gameObject.AddComponent<PlayerDash>();
        
        if (GetComponent<PlayerInteractor>() == null)
            gameObject.AddComponent<PlayerInteractor>();

        currentStamina = maxStamina;
    }

    private void Start()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (playerStats != null)
        {
            MoveSpeed = playerStats.MoveSpeed;
            sprintSpeed = MoveSpeed * 2f;
        }
    }

    private float EffectiveMoveSpeed => playerStats != null ? playerStats.MoveSpeed : MoveSpeed;
    private float EffectiveSprintSpeed => playerStats != null ? playerStats.MoveSpeed * 2f : sprintSpeed;

    /// <summary>
    /// Gọi bởi NetworkPlayer.FixedUpdateNetwork để áp dụng velocity ngay trong Fusion tick.
    /// Tránh race condition với Unity FixedUpdate trong multiplayer.
    /// </summary>
    public void ApplyNetworkVelocity()
    {
        if (playerStats != null && playerStats.IsDead)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (cachedCombat != null && cachedCombat.IsAttacking)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsMoving)
            LastDirection = MoveInput.normalized;

        Rb.linearVelocity = GetVelocity();
    }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprintInputPressed = value.isPressed;
    }

    private void Update()
    {
        if (playerStats != null && playerStats.IsDead)
            return;

        HandleStamina();
    }

    private void HandleStamina()
    {
        float effectiveDrain = playerStats != null ? staminaDrainRate / playerStats.StaminaEfficiency : staminaDrainRate;
        float effectiveRegen = playerStats != null ? staminaRegenRate * playerStats.StaminaEfficiency : staminaRegenRate;

        if (IsDashing)
        {
            isSprinting = false;
            RecoverStamina(effectiveRegen * Time.deltaTime);
            return;
        }

        if (isSprintInputPressed && IsMoving && CurrentStamina > 0f)
        {
            isSprinting = true;
            SpendStamina(effectiveDrain * Time.deltaTime);
        }
        else
        {
            isSprinting = false;
            RecoverStamina(effectiveRegen * Time.deltaTime);
        }
    }

    private void SpendStamina(float amount)
    {
        if (playerStats != null)
        {
            playerStats.SpendStamina(Mathf.Min(amount, playerStats.CurrentStamina));
            return;
        }

        currentStamina = Mathf.Max(0f, currentStamina - amount);
    }

    private void RecoverStamina(float amount)
    {
        if (playerStats != null)
        {
            playerStats.RecoverStamina(amount);
            return;
        }

        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }

    protected override Vector2 GetVelocity()
    {
        return MoveInput * (isSprinting ? EffectiveSprintSpeed : EffectiveMoveSpeed);
    }

    protected override void FixedUpdate()
    {
        // Khi được điều khiển bởi Fusion (multiplayer), bỏ qua FixedUpdate của Unity.
        // Velocity đã được áp dụng trong ApplyNetworkVelocity() gọi từ FixedUpdateNetwork.
        if (IsControlledByNetwork)
            return;

        if (playerStats != null && playerStats.IsDead)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsDashing)
            return;

        if (cachedCombat != null && cachedCombat.IsAttacking)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        base.FixedUpdate();
    }
}

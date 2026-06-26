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
    private float currentStamina;
    private bool isSprintInputPressed;
    private bool isSprinting;

    public bool IsSprinting => isSprinting;
    public float CurrentStamina => playerStats != null ? playerStats.CurrentStamina : currentStamina;
    public float MaxStamina => playerStats != null ? playerStats.MaxStamina : maxStamina;

    protected override void Awake()
    {
        base.Awake();
        playerStats = GetComponent<PlayerStats>();
        currentStamina = maxStamina;
    }

    private void Start()
    {
        if (playerStats != null)
        {
            MoveSpeed = playerStats.MoveSpeed;
            sprintSpeed = MoveSpeed * 2f;
        }
    }

    public void RefreshStats()
    {
        if (playerStats != null)
        {
            MoveSpeed = playerStats.MoveSpeed;
            sprintSpeed = MoveSpeed * 2f;
        }
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
        return MoveInput * (isSprinting ? sprintSpeed : MoveSpeed);
    }

    protected override void FixedUpdate()
    {
        if (playerStats != null && playerStats.IsDead)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsAttacking)
        {
            Rb.linearVelocity = Vector2.zero;
            return;
        }

        base.FixedUpdate();
    }
}

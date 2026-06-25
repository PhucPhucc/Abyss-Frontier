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
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

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

    public void RestoreStamina()
    {
        currentStamina = maxStamina;
    }

    public bool SpendStamina(float amount)
    {
        if (amount <= 0f) return true;
        if (currentStamina < amount) return false;

        currentStamina -= amount;
        return true;
    }

    public void RecoverStamina(float amount)
    {
        if (amount <= 0f || currentStamina >= maxStamina) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
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
        float effectiveDrain = staminaDrainRate;
        float effectiveRegen = staminaRegenRate;
        if (playerStats != null)
        {
            effectiveDrain = staminaDrainRate / playerStats.StaminaEfficiency;
            effectiveRegen = staminaRegenRate * playerStats.StaminaEfficiency;
        }

        if (isSprintInputPressed && IsMoving && currentStamina > 0)
        {
            isSprinting = true;
            currentStamina -= effectiveDrain * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0f;
        }
        else
        {
            isSprinting = false;
            if (currentStamina < maxStamina)
            {
                currentStamina += effectiveRegen * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }
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

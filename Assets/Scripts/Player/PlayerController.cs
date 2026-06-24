using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 15f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;

    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;

    private float currentStamina;
    private bool isSprintInputPressed;
    private bool isSprinting;

    public Vector2 LastDirection => lastDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = gameObject.AddComponent<PlayerStats>();
        }

        combat = GetComponent<PlayerCombat>();
        currentStamina = maxStamina;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprintInputPressed = value.isPressed;
    }

    private void Update()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            lastDirection = moveInput.normalized;
        }

        HandleStamina(isMoving);

        if (animator != null)
        {
            animator.SetFloat("moveX", lastDirection.x);
            animator.SetFloat("moveY", lastDirection.y);
            animator.SetBool("isWalk", isMoving);
            animator.SetBool("isRun", isSprinting);
        }

    }

    private void HandleStamina(bool isMoving)
    {
        if (playerStats != null)
        {
            HandlePlayerStatsStamina(isMoving);
            return;
        }

        if (isSprintInputPressed && isMoving && currentStamina > 0)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0f;
        }
        else
        {
            isSprinting = false;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }
    }

    private void HandlePlayerStatsStamina(bool isMoving)
    {
        if (isSprintInputPressed && isMoving && playerStats.CurrentStamina > 0f)
        {
            isSprinting = true;
            float staminaCost = Mathf.Min(staminaDrainRate * Time.deltaTime, playerStats.CurrentStamina);
            playerStats.SpendStamina(staminaCost);
        }
        else
        {
            isSprinting = false;
            playerStats.RecoverStamina(staminaRegenRate * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (combat != null && combat.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        rb.linearVelocity = moveInput * currentSpeed;
    }
}

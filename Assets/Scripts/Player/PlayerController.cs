using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Điều khiển chuyển động, stamina và hoạt ảnh của Player.
/// Sử dụng PlayerInput component và Input System mới.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;    // Tốc độ đi bộ
    [SerializeField] private float sprintSpeed = 6f;  // Tốc độ chạy nước rút

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;       // Stamina tối đa
    [SerializeField] private float staminaDrainRate = 20f;  // Tốc độ tiêu hao stamina khi sprint
    [SerializeField] private float staminaRegenRate = 15f;  // Tốc độ hồi stamina

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStats playerStats;

    private Vector2 moveInput;                     // Input di chuyển (x, y)
    private Vector2 lastDirection = Vector2.down;  // Hướng cuối cùng (dùng cho animation idle)

    private float currentStamina;         // Stamina hiện tại
    private bool isSprintInputPressed;    // Trạng thái nút Sprint
    private bool isSprinting;             // Player có đang chạy hay không

    /// <summary>Hướng di chuyển cuối cùng (dùng bởi PlayerCombat để xác định hướng tấn công)</summary>
    public Vector2 LastDirection => lastDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        currentStamina = maxStamina;
    }

    private void Start()
    {
        if (playerStats != null)
        {
            moveSpeed = playerStats.MoveSpeed;
            sprintSpeed = moveSpeed * 2f;
        }
    }

    /// <summary>
    /// Làm mới tốc độ từ PlayerStats (gọi khi nâng cấp chỉ số ở Hub).
    /// </summary>
    public void RefreshStats()
    {
        if (playerStats != null)
        {
            moveSpeed = playerStats.MoveSpeed;
            sprintSpeed = moveSpeed * 2f;
        }
    }

    /// <summary>Nhận input di chuyển từ PlayerInput.</summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /// <summary>Nhận input sprint từ PlayerInput.</summary>
    public void OnSprint(InputValue value)
    {
        isSprintInputPressed = value.isPressed;
    }

    /// <summary>Kích hoạt animation bị thương (hurt).</summary>
    public void TriggerHurt()
    {
        if (animator != null)
            animator.SetTrigger("hurt");
    }

    private void Update()
    {
        // Nếu Player chết, dừng mọi hoạt ảnh di chuyển
        if (playerStats != null && playerStats.IsDead)
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
            return;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            lastDirection = moveInput.normalized;
        }

        HandleStamina(isMoving);

        // Cập nhật tham số Animator
        animator.SetFloat("moveX", lastDirection.x);
        animator.SetFloat("moveY", lastDirection.y);
        animator.SetBool("isWalk", isMoving);
        animator.SetBool("isRun", isSprinting);
    }

    /// <summary>
    /// Xử lý stamina: tiêu hao khi sprint, hồi phục khi không sprint.
    /// Stamina dùng chung cho cả Sprint và Dodge.
    /// </summary>
    private void HandleStamina(bool isMoving)
    {
        if (playerStats != null)
        {
            float effectiveDrain = staminaDrainRate / playerStats.StaminaEfficiency;
            float effectiveRegen = staminaRegenRate * playerStats.StaminaEfficiency;

            if (isSprintInputPressed && isMoving && playerStats.CurrentStamina > 0f)
            {
                isSprinting = true;
                float staminaCost = Mathf.Min(effectiveDrain * Time.deltaTime, playerStats.CurrentStamina);
                playerStats.SpendStamina(staminaCost);
            }
            else
            {
                isSprinting = false;
                playerStats.RecoverStamina(effectiveRegen * Time.deltaTime);
            }
            return;
        }

        // Fallback nếu không có PlayerStats
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

    private void FixedUpdate()
    {
        // Khi chết, đứng im
        if (playerStats != null && playerStats.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Khi đang tấn công, đứng im (không cho di chuyển)
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        rb.linearVelocity = moveInput * currentSpeed;
    }
}

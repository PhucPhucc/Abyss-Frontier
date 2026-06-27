using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Dash burst tiêu stamina, có i-frames và afterimage. Không cần animation riêng.
/// </summary>
[RequireComponent(typeof(CharacterMotor), typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashDistance = 2.5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float staminaCost = 20f;
    [SerializeField] private float cooldown = 0.4f;

    [Header("Invulnerability")]
    [SerializeField] private float invulnerabilityDuration = 0.25f;

    [Header("Visual")]
    [SerializeField] private float afterimageInterval = 0.04f;
    [SerializeField] private float dashAnimatorSpeed = 1.75f;

    private CharacterMotor motor;
    private Rigidbody2D rb;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private Animator animator;

    private bool isDashing;
    private float dashTimer;
    private Vector2 dashVelocity;
    private float lastDashTime = -999f;
    private float afterimageTimer;
    private float originalAnimatorSpeed = 1f;
    private Coroutine invulnerabilityRoutine;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        motor = GetComponent<CharacterMotor>();
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
        animator = GetComponentInChildren<Animator>();
    }

    public void OnDodge(InputValue value)
    {
        if (!value.isPressed)
            return;

        TryDash();
    }

    public bool TryDash()
    {
        if (isDashing)
            return false;

        if (playerStats != null && playerStats.IsDead)
            return false;

        if (Time.time < lastDashTime + cooldown)
            return false;

        if (playerStats != null && !playerStats.SpendStamina(staminaCost))
            return false;

        Vector2 direction = GetDashDirection();
        BeginDash(direction);
        return true;
    }

    private Vector2 GetDashDirection()
    {
        if (motor.IsMoving)
            return motor.MoveInput.normalized;

        Vector2 facing = motor.LastDirection;
        return facing.sqrMagnitude > 0.01f ? facing.normalized : Vector2.down;
    }

    private void BeginDash(Vector2 direction)
    {
        motor.SetLastDirection(direction);

        isDashing = true;
        dashTimer = dashDuration;
        dashVelocity = direction * (dashDistance / dashDuration);
        afterimageTimer = 0f;
        lastDashTime = Time.time;

        if (animator != null)
        {
            originalAnimatorSpeed = animator.speed;
            animator.speed = dashAnimatorSpeed;
        }

        DashAfterimage.SpawnFromCharacter(transform);

        if (invulnerabilityRoutine != null)
            StopCoroutine(invulnerabilityRoutine);
        invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine());
    }

    private void FixedUpdate()
    {
        if (!isDashing)
            return;

        rb.linearVelocity = dashVelocity;
        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
            EndDash();
    }

    private void Update()
    {
        if (!isDashing)
            return;

        afterimageTimer -= Time.deltaTime;
        if (afterimageTimer <= 0f)
        {
            DashAfterimage.SpawnFromCharacter(transform);
            afterimageTimer = afterimageInterval;
        }
    }

    private void EndDash()
    {
        if (!isDashing)
            return;

        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.speed = originalAnimatorSpeed;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        playerHealth?.SetInvulnerable(true);
        yield return new WaitForSeconds(invulnerabilityDuration);
        playerHealth?.SetInvulnerable(false);
        invulnerabilityRoutine = null;
    }
}

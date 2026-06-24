using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Xử lý tấn công của Player: quét hitbox, gây sát thương, knockback.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] private int attackDamage = 10;     // Sát thương cơ bản (dùng khi không có PlayerStats)
    [SerializeField] private float attackRange = 0.8f;  // Bán kính hitbox tấn công
    [SerializeField] private float attackCooldown = 0.5f; // Thời gian hồi giữa các đòn

    [Header("Hitbox Setup")]
    [SerializeField] private float hitboxOffset = 0.6f;    // Khoảng cách hitbox so với Player (theo hướng mặt)
    [SerializeField] private LayerMask enemyLayers;         // Layer chứa Enemy để quét hitbox
    [SerializeField] private float attackHitDelay = 0.2f;   // Độ trễ giữa trigger animation và quét hitbox

    private Animator animator;
    private PlayerController playerController;
    private PlayerStats playerStats;
    private InputAction attackAction;
    private float nextAttackTime = 0f;   // Thời điểm được phép tấn công tiếp theo (cooldown)

    /// <summary>Player có đang trong thời gian cooldown tấn công không?</summary>
    public bool IsAttacking => Time.time < nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();

        // Lấy InputAction "Attack" từ PlayerInput
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            attackAction = playerInput.actions.FindAction("Attack");
        }
    }

    private void Update()
    {
        // Kiểm tra input Attack và cooldown
        if (attackAction != null && attackAction.WasPressedThisFrame() && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    /// <summary>
    /// Thực hiện tấn công: kích hoạt animation, sau delay sẽ quét hitbox.
    /// </summary>
    private void PerformAttack()
    {
        animator.SetTrigger("Attack");

        if (attackHitDelay > 0f)
        {
            StartCoroutine(AttackDelayRoutine());
        }
        else
        {
            TriggerAttackDamage();
        }
    }

    /// <summary>
    /// Coroutine chờ attackHitDelay giây rồi quét hitbox.
    /// </summary>
    private System.Collections.IEnumerator AttackDelayRoutine()
    {
        yield return new WaitForSeconds(attackHitDelay);
        TriggerAttackDamage();
    }

    /// <summary>
    /// Quét hitbox hình tròn phía trước mặt Player và gây sát thương cho Enemy trúng đòn.
    /// Có thể gọi từ Animation Event nếu cần chính xác theo frame.
    /// </summary>
    public void TriggerAttackDamage()
    {
        if (playerController == null) return;

        Vector2 facingDirection = playerController.LastDirection;
        Vector2 attackPoint = (Vector2)transform.position + (facingDirection * hitboxOffset);

        // Quét tất cả Enemy trong vùng đòn đánh
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayers);

        int damage = playerStats != null ? playerStats.AttackDamage : attackDamage;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Tính hướng knockback từ Player đến Enemy
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                if (knockbackDir == Vector2.zero)
                    knockbackDir = facingDirection;

                enemyHealth.TakeDamage(damage, knockbackDir);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ hitbox trong Editor (chỉ khi đang play)
        if (Application.isPlaying && playerController != null)
        {
            Vector2 attackPoint = (Vector2)transform.position + (playerController.LastDirection * hitboxOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint, attackRange);
        }
    }
}

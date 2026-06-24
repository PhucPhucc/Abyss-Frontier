using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 0.5f;
    
    [Header("Hitbox Setup")]
    [SerializeField] private float hitboxOffset = 0.6f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackHitDelay = 0.2f; // Độ trễ (giây) từ lúc vung kiếm đến lúc gây sát thương. Đặt = 0 nếu dùng Animation Event.

    private Animator animator;
    private PlayerController playerController;
    private InputAction attackAction;
    private float nextAttackTime = 0f;
    public bool IsAttacking => Time.time < nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            attackAction = playerInput.actions.FindAction("Attack");
        }
    }

    private void Update()
    {
        if (attackAction != null && attackAction.WasPressedThisFrame() && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void PerformAttack()
    {
        animator.SetTrigger("Attack");

        // Nếu có độ trễ delay > 0, ta chạy Coroutine. Nếu = 0, người chơi sẽ tự dùng Animation Event để gọi TriggerAttackDamage()
        if (attackHitDelay > 0f)
        {
            StartCoroutine(AttackDelayRoutine());
        }
    }

    private System.Collections.IEnumerator AttackDelayRoutine()
    {
        yield return new WaitForSeconds(attackHitDelay);
        TriggerAttackDamage();
    }

    /// <summary>
    /// Thực hiện quét hitbox gây sát thương và đẩy lùi Enemy.
    /// Có thể được gọi từ Coroutine hoặc trực tiếp bằng Animation Event trong Unity Editor.
    /// </summary>
    public void TriggerAttackDamage()
    {
        if (playerController == null) return;

        Vector2 facingDirection = playerController.LastDirection;
        Vector2 attackPoint = (Vector2)transform.position + (facingDirection * hitboxOffset);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Tính toán hướng đẩy lùi: từ Player tới Enemy
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                if (knockbackDir == Vector2.zero)
                {
                    knockbackDir = facingDirection;
                }

                // Gọi TakeDamage với lực đẩy lùi
                enemyHealth.TakeDamage(attackDamage, knockbackDir);
            }
            else
            {
                Debug.LogWarning($"[PlayerCombat] {enemy.name} không có EnemyHealth component!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && playerController != null)
        {
            Vector2 attackPoint = (Vector2)transform.position + (playerController.LastDirection * hitboxOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint, attackRange);
        }
    }
}

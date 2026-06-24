using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Stats")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 0.5f;
    
    [Header("Hitbox Setup")]
    [SerializeField] private float hitboxOffset = 0.6f;
    [SerializeField] private LayerMask enemyLayers;

    private Animator animator;
    private PlayerController playerController;
    private PlayerStats playerStats;
    private InputAction attackAction;
    private float nextAttackTime = 0f;
    public bool IsAttacking => Time.time < nextAttackTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = gameObject.AddComponent<PlayerStats>();
        }

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
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Vector2 facingDirection = playerController != null ? playerController.LastDirection : Vector2.down;
        Vector2 attackPoint = (Vector2)transform.position + (facingDirection * hitboxOffset);
        int currentDamage = playerStats != null ? playerStats.AttackDamage : attackDamage;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log($"Hit: {enemy.name} for {currentDamage} damage");
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

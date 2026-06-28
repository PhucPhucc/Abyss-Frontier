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
    [SerializeField] private float attackHitDelay = 0.2f;

    private PlayerController playerController;
    private PlayerStats playerStats;
    private PlayerDash playerDash;
    private CharacterAnimationHandler animHandler;
    private InputAction attackAction;
    private float nextAttackTime = 0f;

    public bool IsAttacking => Time.time < nextAttackTime;
    public bool UseNetworkInput { get; set; }
    public void SetAttackCooldown(float duration) => nextAttackTime = Time.time + duration;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        playerDash = GetComponent<PlayerDash>();
        animHandler = GetComponent<CharacterAnimationHandler>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            attackAction = playerInput.actions.FindAction("Attack");
        }
    }

    private void Update()
    {
        if (UseNetworkInput) return;
        if (playerDash != null && playerDash.IsDashing)
            return;
        if (attackAction != null && attackAction.WasPressedThisFrame() && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void PerformAttack()
    {
        AudioManager.Instance?.PlayPlayerAttack();
        animHandler?.TriggerAttack();

        if (attackHitDelay > 0f)
        {
            StartCoroutine(AttackDelayRoutine());
        }
        else
        {
            TriggerAttackDamage();
        }
    }

    private System.Collections.IEnumerator AttackDelayRoutine()
    {
        yield return new WaitForSeconds(attackHitDelay);
        // Hủy gây sát thương nếu Player đã chết trong lúc chờ
        if (playerStats == null || !playerStats.IsDead)
            TriggerAttackDamage();
    }

    public void TriggerAttackAnimationOnly()
    {
        animHandler?.TriggerAttack();
    }

    public void TriggerAttackDamage()
    {
        if (playerController == null) return;

        Vector2 facingDirection = playerController.LastDirection;
        Vector2 attackPoint = (Vector2)transform.position + (facingDirection * hitboxOffset);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayers);

        int damage = playerStats != null ? playerStats.AttackDamage : attackDamage;

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                if (knockbackDir == Vector2.zero)
                    knockbackDir = facingDirection;

                enemyHealth.TakeDamage(damage, knockbackDir, source: transform);
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

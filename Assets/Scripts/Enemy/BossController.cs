using System.Collections;
using UnityEngine;

/// <summary>
/// Máy trạng thái AI, lật ảnh tự động và đòn tấn công diện rộng cho Boss Tầng 5 (Minotaur).
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth), typeof(SpriteRenderer))]
public class BossController : MonoBehaviour
{
    private enum BossState { Intro, Chase, Attack, Cooldown, Dead }

    [Header("Boss Parameters")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackHitDelay = 0.5f; // Thời điểm gây sát thương trong animation atk_1
    [SerializeField] private float attackCooldown = 1.8f;
    [SerializeField] private float attackAoERadius = 1.8f;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private EnemyHealth health;
    private Transform target;

    private BossState state = BossState.Intro;
    private float cooldownTimer = 0f;
    private bool isAttacking = false;
    private bool victoryTriggered = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        FindPlayer();
        StartCoroutine(IntroRoutine());
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) p = pc.gameObject;
        }
        if (p != null) target = p.transform;
    }

    private IEnumerator IntroRoutine()
    {
        state = BossState.Intro;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isMoving", false);
        
        yield return new WaitForSeconds(1.5f); // Đứng gầm thét 1.5s
        state = BossState.Chase;
    }

    private void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            if (state != BossState.Dead)
            {
                state = BossState.Dead;
                rb.linearVelocity = Vector2.zero;
                if (!victoryTriggered)
                {
                    victoryTriggered = true;
                    TriggerVictoryUI();
                }
            }
            return;
        }

        if (target == null) FindPlayer();
        if (target == null || state == BossState.Intro || state == BossState.Dead) return;

        cooldownTimer -= Time.fixedDeltaTime;

        // Lật ảnh quay mặt về phía Player (Ảnh gốc vẽ Minotaur hướng sang Trái)
        if (target.position.x > transform.position.x + 0.1f)
            sr.flipX = true;
        else if (target.position.x < transform.position.x - 0.1f)
            sr.flipX = false;

        Vector2 hitCenter = (Vector2)transform.position + (sr.flipX ? Vector2.right : Vector2.left) * 0.8f;
        float dist = Vector2.Distance(hitCenter, target.position);

        if (state == BossState.Chase)
        {
            if (dist <= attackRange && cooldownTimer <= 0f && !isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                Vector2 dir = (target.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
                if (anim != null) anim.SetBool("isMoving", true);
            }
        }
        else if (state == BossState.Cooldown)
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetBool("isMoving", false);
            if (cooldownTimer <= 0f) state = BossState.Chase;
        }
    }

    private IEnumerator AttackRoutine()
    {
        state = BossState.Attack;
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        AudioManager.Instance?.PlayEnemyAttack();
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
            anim.SetTrigger("attack");
        }

        yield return new WaitForSeconds(attackHitDelay);

        // Quét bán kính sát thương diện rộng (AoE)
        if (state != BossState.Dead && target != null)
        {
            Vector2 hitCenter = (Vector2)transform.position + (sr.flipX ? Vector2.right : Vector2.left) * 0.8f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackAoERadius, playerLayer);
            foreach (Collider2D h in hits)
            {
                PlayerStats ps = h.GetComponent<PlayerStats>() ?? h.GetComponentInParent<PlayerStats>();
                if (ps != null)
                {
                    ps.TakeDamage(attackDamage);
                    Debug.Log($"[BossController] Minotaur chém trúng Player — {attackDamage} sát thương!");
                    break;
                }
            }
        }

        yield return new WaitForSeconds(0.6f); // Đợi kết thúc hoạt ảnh chém

        isAttacking = false;
        cooldownTimer = attackCooldown;
        state = BossState.Cooldown;
    }

    private void TriggerVictoryUI()
    {
        Debug.Log("[BossController] Boss Minotaur đã bị tiêu diệt! Kích hoạt Victory UI...");
        BossVictoryUI vicUI = FindFirstObjectByType<BossVictoryUI>(FindObjectsInactive.Include);
        if (vicUI != null)
        {
            vicUI.ShowVictory();
        }
        else
        {
            Debug.LogWarning("[BossController] Không tìm thấy BossVictoryUI trong scene!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 hitCenter = (Vector2)transform.position + (GetComponent<SpriteRenderer>() != null && GetComponent<SpriteRenderer>().flipX ? Vector2.right : Vector2.left) * 0.8f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hitCenter, attackAoERadius);
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// Máy trạng thái AI, lật ảnh tự động và đòn tấn công diện rộng cho boss cuối tầng.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth), typeof(SpriteRenderer))]
public class BossController : MonoBehaviour
{
    protected enum BossState { Intro, Chase, Attack, Cooldown, Dead }

    [Header("Boss Identity")]
    [SerializeField] private string bossDisplayName = "Boss";
    [SerializeField] private bool triggerVictoryOnDeath;
    [SerializeField] private float introDuration = 1.5f;
    [SerializeField] private float facingHitOffset = 0.8f;
    [SerializeField] private bool spriteFacesLeftByDefault = true;

    [Header("Boss Parameters")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackHitDelay = 0.5f;
    [SerializeField] private float attackAnimTailDuration = 0.6f;
    [SerializeField] private float attackCooldown = 1.8f;
    [SerializeField] private float attackAoERadius = 1.8f;
    [SerializeField] private LayerMask playerLayer;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected EnemyHealth health;
    protected Transform target;

    protected BossState state = BossState.Intro;
    protected float cooldownTimer;
    protected bool isAttacking;
    private bool victoryTriggered;

    protected bool IsDead => state == BossState.Dead;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
    }

    protected virtual void Start()
    {
        FindPlayer();
        if (health != null)
            health.Died += OnBossDied;

        StartCoroutine(IntroRoutine());
    }

    protected virtual void OnDestroy()
    {
        if (health != null)
            health.Died -= OnBossDied;
    }

    private void OnBossDied()
    {
        if (state == BossState.Dead) return;

        state = BossState.Dead;
        isAttacking = false;
        StopAllCoroutines();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (!victoryTriggered)
        {
            victoryTriggered = true;
            if (triggerVictoryOnDeath)
                TriggerVictoryUI();
        }
    }

    protected void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) p = pc.gameObject;
        }
        if (p != null) target = p.transform;
    }

    protected virtual IEnumerator IntroRoutine()
    {
        state = BossState.Intro;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isMoving", false);

        yield return new WaitForSeconds(introDuration);
        state = BossState.Chase;
    }

    protected virtual void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            if (state != BossState.Dead)
                OnBossDied();
            return;
        }

        if (target == null) FindPlayer();
        if (target == null || state == BossState.Intro || state == BossState.Dead) return;

        cooldownTimer -= Time.fixedDeltaTime;
        UpdateFacing();

        Vector2 hitCenter = GetHitCenter();
        float dist = Vector2.Distance(hitCenter, target.position);

        if (state == BossState.Chase)
        {
            if (dist <= attackRange && cooldownTimer <= 0f && !isAttacking)
                StartCoroutine(AttackRoutine());
            else
                ChaseTarget();
        }
        else if (state == BossState.Cooldown)
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetBool("isMoving", false);
            if (cooldownTimer <= 0f) state = BossState.Chase;
        }
    }

    protected virtual void ChaseTarget()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        if (anim != null) anim.SetBool("isMoving", true);
    }

    protected virtual void UpdateFacing()
    {
        if (spriteFacesLeftByDefault)
        {
            if (target.position.x > transform.position.x + 0.1f)
                sr.flipX = true;
            else if (target.position.x < transform.position.x - 0.1f)
                sr.flipX = false;
        }
        else
        {
            if (target.position.x > transform.position.x + 0.1f)
                sr.flipX = false;
            else if (target.position.x < transform.position.x - 0.1f)
                sr.flipX = true;
        }
    }

    protected Vector2 GetHitCenter()
    {
        Vector2 facing = sr.flipX ? Vector2.right : Vector2.left;
        if (!spriteFacesLeftByDefault)
            facing = sr.flipX ? Vector2.left : Vector2.right;

        return (Vector2)transform.position + facing * facingHitOffset;
    }

    protected virtual IEnumerator AttackRoutine()
    {
        state = BossState.Attack;
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        AudioManager.Instance?.PlayEnemyAttack();
        if (anim != null)
        {
            anim.SetBool("isMoving", false);
            anim.SetTrigger(GetAttackTrigger());
        }

        yield return new WaitForSeconds(attackHitDelay);

        if (!IsDead && target != null)
            ApplyAttackDamage();

        yield return new WaitForSeconds(attackAnimTailDuration);

        isAttacking = false;
        cooldownTimer = attackCooldown;
        state = BossState.Cooldown;
    }

    protected virtual string GetAttackTrigger() => "attack";

    protected virtual void ApplyAttackDamage()
    {
        Vector2 hitCenter = GetHitCenter();
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, attackAoERadius, playerLayer);
        foreach (Collider2D h in hits)
        {
            PlayerStats ps = h.GetComponent<PlayerStats>() ?? h.GetComponentInParent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(attackDamage);
                Debug.Log($"[BossController] {bossDisplayName} đánh trúng Player — {attackDamage} sát thương!");
                break;
            }
        }
    }

    private void TriggerVictoryUI()
    {
        Debug.Log($"[BossController] {bossDisplayName} đã bị tiêu diệt! Kích hoạt Victory UI...");
        BossVictoryUI vicUI = FindFirstObjectByType<BossVictoryUI>(FindObjectsInactive.Include);
        if (vicUI != null)
            vicUI.ShowVictory();
        else
            Debug.LogWarning("[BossController] Không tìm thấy BossVictoryUI trong scene!");
    }

    private void OnDrawGizmosSelected()
    {
        SpriteRenderer gizmoSr = GetComponent<SpriteRenderer>();
        if (gizmoSr == null) return;

        Vector2 facing = gizmoSr.flipX ? Vector2.right : Vector2.left;
        if (!spriteFacesLeftByDefault)
            facing = gizmoSr.flipX ? Vector2.left : Vector2.right;

        Vector2 hitCenter = (Vector2)transform.position + facing * facingHitOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(hitCenter, attackAoERadius);
    }
}

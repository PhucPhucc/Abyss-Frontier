using System.Collections;
using UnityEngine;

/// <summary>
/// Máy trạng thái AI, lật ảnh tự động và đòn tấn công diện rộng cho boss cuối tầng.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth), typeof(SpriteRenderer))]
public class BossController : MonoBehaviour
{
    protected enum BossState { Intro, Idle, Chase, Attack, Cooldown, ReturnHome, Dead }

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

    [Header("Detection / Leash")]
    [Tooltip("Player must enter this range (and leash) before the boss starts chasing.")]
    [SerializeField] private float detectionRange = 6f;
    [Tooltip("Boss stops chasing when Player is farther than this.")]
    [SerializeField] private float loseRange = 10f;
    [Tooltip("Boss only engages Player while Player is within this radius of the boss spawn/home.")]
    [SerializeField] private float leashRadius = 8f;
    [SerializeField] private float homeStoppingDistance = 0.15f;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected EnemyHealth health;
    protected Transform target;

    protected BossState state = BossState.Intro;
    protected float cooldownTimer;
    protected bool isAttacking;
    private bool victoryTriggered;
    private Vector2 homePosition;

    protected bool IsDead => state == BossState.Dead;
    private float targetRefreshTimer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
        homePosition = transform.position;
    }

    protected virtual void Start()
    {
        FindPlayer();
        targetRefreshTimer = 0f;
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
        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p == null || !p.activeInHierarchy)
                continue;

            PlayerHealth playerHealth = p.GetComponent<PlayerHealth>() ?? p.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null && playerHealth.IsDead)
                continue;

            float distance = Vector2.Distance(transform.position, p.transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = p.transform;
            }
        }

        if (bestTarget == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                PlayerHealth playerHealth = pc.GetComponent<PlayerHealth>() ?? pc.GetComponentInParent<PlayerHealth>();
                if (playerHealth == null || !playerHealth.IsDead)
                    bestTarget = pc.transform;
            }
        }

        if (bestTarget != null)
            target = bestTarget;
    }

    protected virtual IEnumerator IntroRoutine()
    {
        state = BossState.Intro;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("isMoving", false);

        yield return new WaitForSeconds(introDuration);
        state = BossState.Idle;
    }

    protected virtual void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            if (state != BossState.Dead)
                OnBossDied();
            return;
        }

        if (GameSessionData.IsMultiplayer && !GameSessionData.IsHost)
            return;

        if (targetRefreshTimer <= 0f)
        {
            FindPlayer();
            targetRefreshTimer = 0.5f;
        }
        else
        {
            targetRefreshTimer -= Time.fixedDeltaTime;
        }

        if (target == null || state == BossState.Intro || state == BossState.Dead || state == BossState.Attack)
            return;

        cooldownTimer -= Time.fixedDeltaTime;

        float distToPlayer = Vector2.Distance(transform.position, target.position);
        bool playerInLeash = IsPlayerInsideLeash();

        if (state == BossState.Idle)
        {
            StopMovement();
            if (distToPlayer <= detectionRange && playerInLeash)
                state = BossState.Chase;
            return;
        }

        if (state == BossState.ReturnHome)
        {
            ReturnHome();
            if (distToPlayer <= detectionRange && playerInLeash)
                state = BossState.Chase;
            return;
        }

        if (state == BossState.Chase)
        {
            UpdateFacing();

            if (distToPlayer > loseRange || !playerInLeash)
            {
                BeginReturnHome();
                return;
            }

            Vector2 hitCenter = GetHitCenter();
            float attackDist = Vector2.Distance(hitCenter, target.position);
            if (attackDist <= attackRange && cooldownTimer <= 0f && !isAttacking)
                StartCoroutine(AttackRoutine());
            else
                ChaseTarget();
            return;
        }

        if (state == BossState.Cooldown)
        {
            StopMovement();
            UpdateFacing();
            if (cooldownTimer <= 0f)
            {
                if (distToPlayer > loseRange || !playerInLeash)
                    BeginReturnHome();
                else
                    state = BossState.Chase;
            }
        }
    }

    protected virtual void ChaseTarget()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        if (anim != null) anim.SetBool("isMoving", true);
    }

    private void BeginReturnHome()
    {
        state = BossState.ReturnHome;
        StopMovement();
    }

    private void ReturnHome()
    {
        float distHome = Vector2.Distance(transform.position, homePosition);
        if (distHome <= homeStoppingDistance)
        {
            transform.position = homePosition;
            StopMovement();
            state = BossState.Idle;
            return;
        }

        Vector2 dir = (homePosition - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        if (anim != null) anim.SetBool("isMoving", true);

        if (sr != null)
        {
            if (spriteFacesLeftByDefault)
                sr.flipX = dir.x > 0.1f;
            else
                sr.flipX = dir.x < -0.1f;
        }
    }

    private void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        if (anim != null)
            anim.SetBool("isMoving", false);
    }

    private bool IsPlayerInsideLeash()
    {
        if (target == null) return false;
        return Vector2.Distance(homePosition, target.position) <= leashRadius;
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
        Vector2 home = Application.isPlaying ? homePosition : (Vector2)transform.position;

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.85f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.85f);
        Gizmos.DrawWireSphere(transform.position, loseRange);
        Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.7f);
        Gizmos.DrawWireSphere(home, leashRadius);

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

using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Dead
}

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float chaseSpeed = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float loseRange = 6f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int attackDamage = 8;
    [SerializeField] private float attackHitDelay = 0.2f;  // Độ trễ từ lúc trigger animation đến lúc quét hitbox (giây)
    [SerializeField] private float attackCooldown = 1.4f;
    [SerializeField] private float attackPrepDuration = 0.4f;
    [SerializeField] private LayerMask playerLayer;        // Layer của Player để quét hitbox
    private float attackTimer = 0f;
    private float prepTimer = 0f;
    private bool wasInAttackRange = false;

    private Rigidbody2D rb;
    private Transform target;
    private Animator anim;

    private EnemyState state = EnemyState.Idle;
    private Vector2 lastDirection = Vector2.down;
    private Vector2 moveVelocity;
    private int waypointIndex = 0;
    private float timer = 0f;
    private bool isDead = false;

    public Vector2 MoveVelocity => moveVelocity;
    public Vector2 LastDirection => lastDirection;
    public EnemyState CurrentState => state;
    public bool IsMoving => moveVelocity.sqrMagnitude > 0.01f;
    public bool IsDead => isDead;

    private KnockbackHandler knockback;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        knockback = GetComponent<KnockbackHandler>();
    }

    private void Start()
    {
        timer = 2f;
        FindTarget();
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) player = pc.gameObject;
        }
        if (player != null) target = player.transform;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Nếu bị choáng (stun), ngừng di chuyển tự thân và logic AI
        if (knockback != null && knockback.IsStunned)
        {
            // Nếu không trong trạng thái đẩy lùi nữa nhưng vẫn bị stun, đứng yên tại chỗ
            if (!knockback.IsGettingKnockedBack && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (anim != null)
            {
                anim.SetBool("isMoving", false);
            }
            return;
        }

        if (target == null) FindTarget();

        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

        Tick();
        ApplyMovement();
        UpdateAnimator();
    }

    private void Tick()
    {
        float dist = target != null
            ? Vector2.Distance(transform.position, target.position)
            : Mathf.Infinity;

        switch (state)
        {
            case EnemyState.Idle:
                moveVelocity = Vector2.zero;
                timer += Time.fixedDeltaTime;
                if (timer >= 2f)
                {
                    if (target != null && dist <= detectionRange)
                        SetState(EnemyState.Chase);
                    else if (waypoints.Length > 0)
                        SetState(EnemyState.Patrol);
                    else
                        timer = 0f;
                }
                break;

            case EnemyState.Patrol:
                Patrol();
                if (target != null && dist <= detectionRange)
                    SetState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                Chase(dist);
                if (target == null || dist > loseRange)
                    SetState(waypoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle);
                break;
        }
    }

    private void Patrol()
    {
        if (waypoints.Length == 0)
        {
            SetState(EnemyState.Idle);
            return;
        }

        Transform wp = waypoints[waypointIndex];
        Vector2 dir = (wp.position - transform.position).normalized;
        float d = Vector2.Distance(transform.position, wp.position);

        if (d > 0.2f)
        {
            moveVelocity = dir * moveSpeed;
            lastDirection = dir;
        }
        else
        {
            moveVelocity = Vector2.zero;
            timer += Time.fixedDeltaTime;
            if (timer >= waitTime)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                timer = 0f;
            }
        }
    }
    private void Chase(float distanceToPlayer)
    {
        if (target == null) return;

        Vector2 dir = (target.position - transform.position).normalized;

        if (distanceToPlayer <= attackRange)
        {
            moveVelocity = Vector2.zero; // Đứng yên khi vào tầm đánh
            lastDirection = dir;

            if (anim != null)
            {
                anim.SetFloat("lastMoveX", dir.x);
                anim.SetFloat("lastMoveY", dir.y);
            }

            // Nếu vừa mới bước vào tầm tấn công
            if (!wasInAttackRange)
            {
                wasInAttackRange = true;
                prepTimer = attackPrepDuration; // Kích hoạt thời gian chuẩn bị 1s
            }

            if (prepTimer > 0f)
            {
                // Đang chuẩn bị cho đòn đánh đầu tiên
                prepTimer -= Time.fixedDeltaTime;
                if (prepTimer <= 0f)
                {
                    TriggerAttackAnimation();
                    attackTimer = attackCooldown; // Đặt cooldown 2s
                }
            }
            else
            {
                if (attackTimer <= 0f)
                {
                    TriggerAttackAnimation();
                    attackTimer = attackCooldown;
                }
            }
        }
        else
        {
            moveVelocity = dir * chaseSpeed;
            lastDirection = dir;

            wasInAttackRange = false;
            prepTimer = 0f;
        }
    }

    private void SetState(EnemyState newState)
    {
        state = newState;
        timer = 0f;
    }

    private void ApplyMovement()
    {
        if (rb == null) return;
        if (knockback != null && knockback.IsGettingKnockedBack) return;
        rb.linearVelocity = moveVelocity;
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;

        if (moveVelocity.sqrMagnitude > 0.01f)
        {
            Vector2 animDir = moveVelocity.normalized;

            anim.SetFloat("moveX", animDir.x);
            anim.SetFloat("moveY", animDir.y);
            anim.SetBool("isMoving", true);

            anim.SetFloat("lastMoveX", animDir.x);
            anim.SetFloat("lastMoveY", animDir.y);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    public void TriggerAttackAnimation()
    {
        if (anim != null) anim.SetTrigger("attack");
        // Quét hitbox sau một độ trễ nhỏ để đồng bộ với hoạt ảnh
        StartCoroutine(AttackHitRoutine());
    }

    private System.Collections.IEnumerator AttackHitRoutine()
    {
        if (attackHitDelay > 0f)
            yield return new WaitForSeconds(attackHitDelay);

        TriggerAttackDamage();
    }

    /// <summary>
    /// Quét hitbox xung quanh Enemy để gây sát thương cho Player.
    /// Có thể gọi qua Animation Event thay cho Coroutine nếu cần chính xác theo frame.
    /// </summary>
    public void TriggerAttackDamage()
    {
        // Quét vùng hình tròn xung quanh enemy theo lastDirection
        Vector2 hitPoint = (Vector2)transform.position + lastDirection.normalized * attackRange * 0.6f;
        Collider2D hit = Physics2D.OverlapCircle(hitPoint, attackRange * 0.5f, playerLayer);

        if (hit != null)
        {
            PlayerStats playerStats = hit.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
                Debug.Log($"[EnemyAI] {name} đánh trúng Player — {attackDamage} sát thương.");
            }
        }
    }

    public void TriggerHurtAnimation()
    {
        if (anim != null) anim.SetTrigger("hurt");
    }

    /// <summary>
    /// Reset bộ đếm chuẩn bị tấn công khi bị trúng đòn (bị gián đoạn).
    /// </summary>
    public void ResetAttackTimer()
    {
        wasInAttackRange = false;
        prepTimer = 0f;
    }

    /// <summary>
    /// Gọi bởi EnemyHealth khi enemy chết — dừng toàn bộ AI và physics.
    /// </summary>
    public void OnDeath()
    {
        isDead = true;
        state = EnemyState.Dead;
        moveVelocity = Vector2.zero;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        // Attack range (outer circle)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Attack hitbox (inner hit point)
        Vector2 hitPoint = (Vector2)transform.position + lastDirection.normalized * attackRange * 0.6f;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f); // Orange
        Gizmos.DrawWireSphere(hitPoint, attackRange * 0.5f);
    }
}
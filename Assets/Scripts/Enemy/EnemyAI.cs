using UnityEngine;

/// <summary>
/// Các trạng thái hành vi của Enemy.
/// </summary>
public enum EnemyState
{
    Idle,    // Đứng yên, chờ
    Patrol,  // Đi tuần tra theo waypoint
    Chase,   // Đuổi theo Player
    ReturnHome,
    Dead     // Đã chết
}

/// <summary>
/// Điều khiển hành vi AI cho Enemy: di chuyển, tuần tra, đuổi theo và tấn công Player.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;       // Tốc độ di chuyển khi tuần tra
    [SerializeField] private float chaseSpeed = 2f;      // Tốc độ khi đuổi theo Player

    [Header("Detection")]
    [SerializeField] private float detectionRange = 4f;  // Khoảng cách phát hiện Player
    [SerializeField] private float loseRange = 6f;       // Khoảng cách mất dấu Player

    [Header("Leash")]
    [SerializeField, Min(0.1f)] private float leashRadius = 6f;
    [SerializeField, Min(0.01f)] private float homeStoppingDistance = 0.15f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;      // Danh sách điểm tuần tra
    [SerializeField] private float waitTime = 1.5f;      // Thời gian chờ tại mỗi waypoint

    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;                     // Tầm đánh
    [SerializeField] private int attackDamage = 8;                      // Sát thương mỗi đòn
    [SerializeField] private float attackHitDelay = 0.2f;                // Độ trễ từ lúc trigger animation đến lúc quét hitbox (giây)
    [SerializeField] private float attackCooldown = 1.4f;               // Thời gian hồi giữa các đòn đánh
    [SerializeField] private float attackPrepDuration = 0.4f;           // Thời gian chuẩn bị trước đòn đầu tiên
    [SerializeField] private LayerMask playerLayer;                     // Layer của Player để quét hitbox

    private float attackTimer = 0f;      // Đếm ngược cooldown tấn công
    private float prepTimer = 0f;        // Đếm ngược thời gian chuẩn bị đòn đánh
    private bool wasInAttackRange = false; // Đánh dấu đã từng trong tầm đánh để tránh reset prepTimer

    private Rigidbody2D rb;        // Tham chiếu Rigidbody2D để di chuyển
    private Transform target;      // Mục tiêu (Player) hiện tại
    private Animator anim;         // Tham chiếu Animator để điều khiển hoạt ảnh

    private EnemyState state = EnemyState.Idle;       // Trạng thái hiện tại của Enemy
    private Vector2 lastDirection = Vector2.down;     // Hướng cuối cùng (dùng cho attack hitbox)
    private Vector2 moveVelocity;                      // Vector vận tốc di chuyển
    private int waypointIndex = 0;                     // Chỉ số waypoint hiện tại
    private float timer = 0f;                          // Bộ đếm thời gian đa năng (chờ waypoint, chuyển trạng thái)
    private bool isDead = false;                       // Cờ chết
    private Vector2 homePosition;

    // Public properties để các component khác (Animator, EnemyHealth...) truy xuất
    public Vector2 MoveVelocity => moveVelocity;
    public Vector2 LastDirection => lastDirection;
    public EnemyState CurrentState => state;
    public bool IsMoving => moveVelocity.sqrMagnitude > 0.01f;
    public bool IsDead => isDead;

    private KnockbackHandler knockback; // Xử lý hiệu ứng knockback
    private bool hasMoveParams;         // Cờ kiểm tra Animator có các tham số hướng (moveX, lastMoveX...) hay không

    private void Awake()
    {
        // Cache các component ngay khi khởi tạo
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        knockback = GetComponent<KnockbackHandler>();
        homePosition = transform.position;

        if (anim != null)
        {
            foreach (var param in anim.parameters)
            {
                if (param.name == "lastMoveX")
                {
                    hasMoveParams = true;
                    break;
                }
            }
        }
    }

    private void Start()
    {
        // Khởi tạo timer và tìm Player
        timer = 2f;
        FindTarget();
    }

    /// <summary>
    /// Tìm kiếm đối tượng Player trong scene thông qua Tag hoặc Component.
    /// </summary>
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

        if (GameSessionData.IsMultiplayer && !GameSessionData.IsHost)
            return;

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

        // Nếu chưa có mục tiêu thì tìm lại
        if (target == null) FindTarget();

        // Giảm dần cooldown tấn công
        if (attackTimer > 0f)
            attackTimer -= Time.fixedDeltaTime;

        Tick();
        ApplyMovement();
        UpdateAnimator();
    }

    /// <summary>
    /// Logic chính của FSM: xử lý chuyển đổi giữa các trạng thái Idle / Patrol / Chase.
    /// </summary>
    private void Tick()
    {
        float dist = target != null
            ? Vector2.Distance(transform.position, target.position)
            : Mathf.Infinity;

        switch (state)
        {
            case EnemyState.Idle:
                // Đứng yên, sau 2 giây thì chuyển sang Patrol hoặc Chase nếu có Player trong tầm
                moveVelocity = Vector2.zero;
                timer += Time.fixedDeltaTime;
                if (timer >= 2f)
                {
                    if (target != null && dist <= detectionRange && IsTargetInsideLeash())
                        SetState(EnemyState.Chase);
                    else if (waypoints.Length > 0)
                        SetState(EnemyState.Patrol);
                    else
                        timer = 0f; // Không có waypoint, tiếp tục Idle
                }
                break;

            case EnemyState.Patrol:
                Patrol();
                // Nếu phát hiện Player trong tầm thì chuyển sang Chase
                if (target != null && dist <= detectionRange && IsTargetInsideLeash())
                    SetState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                if (target == null || dist > loseRange || !IsTargetInsideLeash())
                {
                    BeginReturnHome();
                    break;
                }

                Chase(dist);
                break;

            case EnemyState.ReturnHome:
                ReturnHome();
                break;
        }
    }

    /// <summary>
    /// Di chuyển Enemy lần lượt qua các waypoint đã định nghĩa.
    /// Khi đến waypoint, chờ một khoảng thời gian rồi đi tiếp.
    /// </summary>
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
            // Còn cách waypoint xa thì di chuyển về phía nó
            moveVelocity = dir * moveSpeed;
            lastDirection = dir;
        }
        else
        {
            // Đã đến waypoint — chờ waitTime rồi chuyển sang waypoint tiếp theo
            moveVelocity = Vector2.zero;
            timer += Time.fixedDeltaTime;
            if (timer >= waitTime)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                timer = 0f;
            }
        }
    }

    /// <summary>
    /// Đuổi theo Player. Khi ở trong tầm đánh thì dừng lại và thực hiện tấn công.
    /// Có cơ chế prepTimer cho đòn đánh đầu tiên để tránh tấn công ngay lập tức.
    /// </summary>
    private void Chase(float distanceToPlayer)
    {
        if (target == null) return;

        Vector2 dir = (target.position - transform.position).normalized;

        if (distanceToPlayer <= attackRange)
        {
            // Đã vào tầm đánh — dừng lại và chuẩn bị tấn công
            moveVelocity = Vector2.zero;
            lastDirection = dir;

            // Cập nhật hướng mặt cho Animator (quan trọng để hitbox trùng hướng)
            if (anim != null && hasMoveParams)
            {
                anim.SetFloat("lastMoveX", dir.x);
                anim.SetFloat("lastMoveY", dir.y);
            }

            // Lần đầu vào tầm — kích hoạt prepTimer
            if (!wasInAttackRange)
            {
                wasInAttackRange = true;
                prepTimer = attackPrepDuration;
            }

            if (prepTimer > 0f)
            {
                // Đang trong giai đoạn chuẩn bị trước đòn đánh đầu tiên
                prepTimer -= Time.fixedDeltaTime;
                if (prepTimer <= 0f)
                {
                    TriggerAttackAnimation();
                    attackTimer = attackCooldown;
                }
            }
            else
            {
                // Đòn đánh theo chu kỳ — chờ cooldown xong mới đánh tiếp
                if (attackTimer <= 0f)
                {
                    TriggerAttackAnimation();
                    attackTimer = attackCooldown;
                }
            }
        }
        else
        {
            // Ngoài tầm đánh — di chuyển về phía Player
            moveVelocity = dir * chaseSpeed;
            lastDirection = dir;

            wasInAttackRange = false;
            prepTimer = 0f;
        }
    }

    /// <summary>
    /// Chuyển đổi trạng thái và reset timer.
    /// </summary>
    private void SetState(EnemyState newState)
    {
        state = newState;
        timer = 0f;
    }

    /// <summary>
    /// Áp dụng vận tốc di chuyển lên Rigidbody2D.
    /// Bỏ qua nếu đang trong trạng thái knockback (để knockback handler tự xử lý).
    /// </summary>
    private void ApplyMovement()
    {
        if (rb == null) return;
        if (knockback != null && knockback.IsGettingKnockedBack) return;
        rb.linearVelocity = moveVelocity;
    }

    /// <summary>
    /// Cập nhật các tham số Animator dựa trên hướng di chuyển và trạng thái.
    /// </summary>
    private void UpdateAnimator()
    {
        if (anim == null) return;

        if (moveVelocity.sqrMagnitude > 0.01f)
        {
            Vector2 animDir = moveVelocity.normalized;

            if (hasMoveParams)
            {
                anim.SetFloat("moveX", animDir.x);
                anim.SetFloat("moveY", animDir.y);
                anim.SetFloat("lastMoveX", animDir.x);
                anim.SetFloat("lastMoveY", animDir.y);
            }
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    /// <summary>
    /// Kích hoạt animation tấn công và bắt đầu Coroutine quét hitbox sau một độ trễ.
    /// </summary>
    public void TriggerAttackAnimation()
    {
        AudioManager.Instance?.PlayEnemyAttack();
        if (anim != null) anim.SetTrigger("attack");
        StartCoroutine(AttackHitRoutine());
    }

    /// <summary>
    /// Coroutine chờ attackHitDelay giây rồi mới quét hitbox gây sát thương.
    /// Giúp đồng bộ sát thương với khung hình hoạt ảnh.
    /// </summary>
    private System.Collections.IEnumerator AttackHitRoutine()
    {
        if (attackHitDelay > 0f)
            yield return new WaitForSeconds(attackHitDelay);

        // Không gây sát thương nếu enemy đã chết trong lúc chờ
        if (!isDead)
            TriggerAttackDamage();
    }

    /// <summary>
    /// Quét hitbox hình tròn phía trước mặt Enemy (theo lastDirection) để gây sát thương cho Player.
    /// Có thể gọi qua Animation Event thay cho Coroutine nếu cần chính xác theo frame.
    /// </summary>
    public void TriggerAttackDamage()
    {
        if (isDead) return;

        // Tính toán vị trí trung tâm của hitbox, lệch về phía trước mặt enemy
        Vector2 hitPoint = (Vector2)transform.position + lastDirection.normalized * attackRange * 0.6f;
        Collider2D hit = Physics2D.OverlapCircle(hitPoint, attackRange * 0.5f, playerLayer);

        if (hit != null)
        {
            if (GameSessionData.IsMultiplayer)
            {
                // Trong multiplayer: gọi RPC để server gửi damage tới đúng client sở hữu player.
                // Không thể gọi playerStats.TakeDamage() trực tiếp vì nó chỉ chạy trên server copy.
                NetworkPlayer netPlayer = hit.GetComponent<NetworkPlayer>()
                                      ?? hit.GetComponentInParent<NetworkPlayer>();
                if (netPlayer != null)
                {
                    netPlayer.RPC_TakeDamage(attackDamage);
                    Debug.Log($"[EnemyAI] {name} gọi RPC_TakeDamage({attackDamage}) → {hit.name}");
                }
            }
            else
            {
                // Singleplayer: gọi trực tiếp
                PlayerStats playerStats = hit.GetComponent<PlayerStats>()
                                       ?? hit.GetComponentInParent<PlayerStats>();

                if (playerStats != null)
                {
                    playerStats.TakeDamage(attackDamage);
                    Debug.Log($"[EnemyAI] {name} đánh trúng Player — {attackDamage} sát thương.");
                }
                else
                {
                    Debug.LogWarning($"[EnemyAI] Hit {hit.name} nhưng không tìm thấy PlayerStats!");
                }
            }
        }
    }

    /// <summary>
    /// Kích hoạt animation bị trúng đòn (hurt).
    /// </summary>
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
    /// Thiết lập chỉ số từ ScriptableObject EnemyStats theo cấp độ tương ứng.
    /// </summary>
    public void SetStatsFromDefinition(EnemyStats stats, int level)
    {
        moveSpeed = stats.GetSpeed(level);
        chaseSpeed = moveSpeed * 1.5f;
        attackDamage = stats.GetATK(level);
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

    /// <summary>
    /// Reset AI after hub respawn so the enemy can move and be fought again.
    /// </summary>
    public void RestoreLivingState()
    {
        isDead = false;
        state = EnemyState.Idle;
        moveVelocity = Vector2.zero;
        attackTimer = 0f;
        prepTimer = 0f;
        wasInAttackRange = false;
        target = null;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        FindTarget();
    }

    /// <summary>
    /// Kiểm tra xem Player có còn trong phạm vi leash (dây xích) so với vị trí home hay không.
    /// </summary>
    private bool IsTargetInsideLeash()
    {
        if (target == null) return false;
        float distFromHome = Vector2.Distance(target.position, homePosition);
        return distFromHome <= leashRadius;
    }

    /// <summary>
    /// Bắt đầu trạng thái quay về vị trí ban đầu (home).
    /// </summary>
    private void BeginReturnHome()
    {
        SetState(EnemyState.ReturnHome);
        moveVelocity = Vector2.zero;
        wasInAttackRange = false;
        prepTimer = 0f;
    }

    /// <summary>
    /// Di chuyển Enemy về vị trí home. Khi đến nơi thì chuyển sang Idle (hoặc Patrol nếu có waypoint).
    /// </summary>
    private void ReturnHome()
    {
        Vector2 dir = (homePosition - (Vector2)transform.position).normalized;
        float dist = Vector2.Distance(transform.position, homePosition);

        if (dist > homeStoppingDistance)
        {
            moveVelocity = dir * moveSpeed;
            lastDirection = dir;
        }
        else
        {
            // Đã về đến home — dừng lại và chuyển về Idle (hoặc Patrol)
            moveVelocity = Vector2.zero;
            SetState(waypoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle);

            // Reset lại timer để có khoảng dừng trước khi hành động tiếp
            timer = 0f;
        }

        // Trong khi đang về nhà, nếu phát hiện Player lại trong tầm thì chase tiếp
        if (target != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, target.position);
            if (distToPlayer <= detectionRange && IsTargetInsideLeash())
                SetState(EnemyState.Chase);
        }
    }

    /// <summary>
    /// Gọi khi Enemy bị Player tấn công — chuyển sang trạng thái Chase ngay lập tức.
    /// </summary>
    public void OnHit(Transform attacker)
    {
        if (isDead) return;

        target = attacker;

        // Chỉ chuyển sang Chase nếu không đang trong trạng thái Dead
        if (state != EnemyState.Dead)
        {
            SetState(EnemyState.Chase);
            wasInAttackRange = false;
            prepTimer = 0f;
        }
    }

    /// <summary>
    /// Vẽ Gizmos trong Editor để hỗ trợ debug: detection range, lose range, attack range.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        // Vòng tròn tầm đánh (outer)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vị trí hitbox thực tế khi tấn công (inner — phía trước mặt)
        Vector2 hitPoint = (Vector2)transform.position + lastDirection.normalized * attackRange * 0.6f;
        Gizmos.color = new Color(1f, 0.45f, 0f, 0.9f); // Cam
        Gizmos.DrawWireSphere(hitPoint, attackRange * 0.5f);
    }
}

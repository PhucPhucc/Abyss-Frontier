using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase
}

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float loseRange = 6f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime = 1.5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float attackTimer = 0f;

    private Rigidbody2D rb;
    private Transform target;
    private Animator anim;

    private EnemyState state = EnemyState.Idle;
    private Vector2 lastDirection = Vector2.down;
    private Vector2 moveVelocity;
    private int waypointIndex = 0;
    private float timer = 0f;

    public Vector2 MoveVelocity => moveVelocity;
    public Vector2 LastDirection => lastDirection;
    public EnemyState CurrentState => state;
    public bool IsMoving => moveVelocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
            moveVelocity = Vector2.zero;

            lastDirection = dir;
            if (anim != null)
            {
                anim.SetFloat("lastMoveX", dir.x);
                anim.SetFloat("lastMoveY", dir.y);
            }

            if (attackTimer <= 0f)
            {
                TriggerAttackAnimation();
                attackTimer = attackCooldown;
            }
        }
        else
        {
            moveVelocity = dir * chaseSpeed;
            lastDirection = dir;
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
    }

    public void TriggerHurtAnimation()
    {
        if (anim != null) anim.SetTrigger("hurt");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
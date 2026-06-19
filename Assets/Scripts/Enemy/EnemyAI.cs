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
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float loseRange = 10f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime = 1.5f;

    private Rigidbody2D rb;
    private Transform target;

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
        Tick();
        ApplyMovement();
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
                Chase();
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

    private void Chase()
    {
        if (target == null) return;
        Vector2 dir = (target.position - transform.position).normalized;
        moveVelocity = dir * chaseSpeed;
        lastDirection = dir;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}

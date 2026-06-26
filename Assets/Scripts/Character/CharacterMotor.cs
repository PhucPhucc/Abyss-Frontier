using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    protected Rigidbody2D rb;

    public Vector2 MoveInput { get; set; }
    public Vector2 LastDirection { get; protected set; } = Vector2.down;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public bool IsMoving => MoveInput.sqrMagnitude > 0.01f;
    public Rigidbody2D Rb => rb;

    public void SetLastDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
            LastDirection = direction.normalized;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual Vector2 GetVelocity()
    {
        return MoveInput * moveSpeed;
    }

    protected virtual void FixedUpdate()
    {
        if (IsMoving)
            LastDirection = MoveInput.normalized;

        rb.linearVelocity = GetVelocity();
    }
}

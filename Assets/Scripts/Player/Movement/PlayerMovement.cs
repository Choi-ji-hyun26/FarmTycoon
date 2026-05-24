using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputReader))]
public class PlayerMovement : Controllable
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private float currentSpeed;
    //private bool isGrounded;

    public float CurrentSpeed => currentSpeed;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void FixedUpdate()
    {
        if (!enabled) return;
        //CheckGround();
        Vector2 input = inputReader.MoveInput;
        Move(input);
        Rotate(input);
    }

    // private void CheckGround()
    // {
    //     isGrounded = Physics.Raycast(
    //         transform.position,
    //         Vector3.down,
    //         groundCheckDistance + 0.1f,
    //         groundLayer
    //     );
    // }

    public override void Move(Vector2 input)
    {
        Vector3 horizontal = new Vector3(input.x, 0f, input.y).normalized * moveSpeed;
        Vector3 velocity = horizontal;
        velocity.y = rb.velocity.y; // 중력은 Rigidbody가 처리
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        currentSpeed = horizontal.magnitude;
    }

    public override void Rotate(Vector2 input)
    {
        if (input.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.fixedDeltaTime));
    }

    public override float GetSpeed() => currentSpeed;

    public override void SetActive(bool active)
    {
        enabled = active;
        rb.isKinematic = !active;

        if (!active)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleMovement : Controllable
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Seat")]
    [SerializeField] private Transform seatPoint;
    public Transform SeatPoint => seatPoint;

    private Transform mountedPlayer;
    private Rigidbody rb;
    private float currentSpeed;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (!enabled) return;
        Vector2 input = inputReader.MoveInput;
        Move(input);
        Rotate(input);

        // 매 프레임 플레이어를 SeatPoint에 동기화
        if (mountedPlayer != null)
        {
            mountedPlayer.position = seatPoint.position;
            // 차 회전 + 180도 오프셋 제거한 방향으로 플레이어 회전 맞춤
            mountedPlayer.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + 180f, 0f);
        }
    }

    public override void Move(Vector2 input)
    {
        Vector3 velocity = new Vector3(input.x, 0f, input.y).normalized * moveSpeed;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        currentSpeed = velocity.magnitude;
    }

    public override void Rotate(Vector2 input)
    {
        if (input.sqrMagnitude < 0.001f) return;
        Quaternion target = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
        // 트렉터 에셋이 반대를 향하고 있음 -> 180도 오프셋 추가
        target *= Quaternion.Euler(0f, 180f, 0f);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.fixedDeltaTime));
    }

    public override float GetSpeed() => currentSpeed;

    public void SetMountedPlayer(Transform player)
    {
        mountedPlayer = player;
    }

    public override void SetActive(bool active)
    {
        enabled = active;
        rb.isKinematic = !active;
    }
}
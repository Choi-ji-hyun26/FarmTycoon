
using UnityEngine;

/*
역할
1. 머신과 데스크 사이 왕복 이동
2. 머신에서 수프를 가능한 만큼 가져오기
3. 데스크에 1개씩 내려놓음
4. 머신이 비면 머신 앞 대기
5. 데스크가 가득 차면 데스크 앞 대기
6. 들고 있는 수량을 비주얼로 반영
*/
public class CourierWorker : MonoBehaviour
{
    private enum CourierState
    {
        GoingToMachine,
        WaitingAtMachine,
        GoingToDesk,
        WaitingAtDesk,
        Depositing
    }

    [Header("Route")]
    [SerializeField] private Transform machinePoint;
    [SerializeField] private Transform deskInputPoint;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Carry")]
    [SerializeField] private int carryCapacity = 10;
    [SerializeField] private int currentCarryCount = 0;

    [Header("Timing")]
    [SerializeField] private float depositInterval = 0.2f;

    [Header("References")]
    [SerializeField] private CookingMachineController cookingMachineController;
    [SerializeField] private SaleDeskController saleDeskController;
    [SerializeField] private WorkerStackController stackController;
    [SerializeField] private Animator animator;

    private CourierState currentState = CourierState.GoingToMachine;
    private float depositTimer;
    private bool isActiveWorker;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int HasCarryHash = Animator.StringToHash("HasCarry");

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActiveWorker)
            return;

        switch (currentState)
        {
            case CourierState.GoingToMachine:
                MoveToMachine();
                break;
            case CourierState.WaitingAtMachine:
                WaitAtMachine();
                break;
            case CourierState.GoingToDesk:
                MoveToDesk();
                break;
            case CourierState.WaitingAtDesk:
                WaitAtDesk();
                break;
            case CourierState.Depositing:
                DepositToDesk();
                break;
        }

        UpdateAnimation();
    }

    // 배달 인력 활성화하고 머신으로 이동 시작
    public void ActivateWorker()
    {
        gameObject.SetActive(true);

        isActiveWorker = true;
        currentState = CourierState.GoingToMachine;
        currentCarryCount = 0;
        depositTimer = 0f;

        if (stackController != null)
            stackController.ClearproductStack();

        UpdateAnimation();
    }

    // 머신 지점으로 이동, 도착하면 픽업 시도
    private void MoveToMachine()
    {
        if (machinePoint == null)
            return;

        MoveToTarget(machinePoint);

        if (IsArrived(machinePoint))
            TryPickupFromMachine();
    }

    // 머신 앞에서 대기, 수프가 생성되면 다시 집음
    private void WaitAtMachine()
    {
        TryPickupFromMachine();
    }

    // 데스크 수납 포인트로 이동, 도착하면 적재 시작
    private void MoveToDesk()
    {
        if (deskInputPoint == null)
            return;

        MoveToTarget(deskInputPoint);

        if (IsArrived(deskInputPoint))
        {
            AlignToDeskForward();
            currentState = CourierState.Depositing;
            depositTimer = 0f;
        }
    }

    private void AlignToDeskForward()
    {
        if (deskInputPoint == null)
            return;

        Vector3 forward = -deskInputPoint.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(forward);
    }

    // 데스크 앞에서 대기
    // 들고 있는 수프가 있고 데스크가 비워지면 다시 적재, 재고도 없으면 머신으로 이동
    private void WaitAtDesk()
    {
        if (saleDeskController == null)
            return;

        if (currentCarryCount > 0)
        {
            if (saleDeskController.CanAddSoup(1))
            {
                currentState = CourierState.Depositing;
                depositTimer = 0f;
                return;
            }
        }

        if (saleDeskController.StoredSoupCount > 0)
            return;

        currentState = CourierState.GoingToMachine;
    }

    // 머신에서 들 수 있는 만큼 수프 집음
    // 재고 없으면 머신 앞 대기, 픽업 성공 시 데스크로 이동
    private void TryPickupFromMachine()
    {
        if (cookingMachineController == null)
            return;

        int canCarry = carryCapacity - currentCarryCount;
        if (canCarry <= 0)
        {
            currentState = CourierState.GoingToDesk;
            return;
        }

        int actualTake = Mathf.Min(canCarry, cookingMachineController.AvailableCount);

        if (actualTake <= 0)
        {
            currentState = CourierState.WaitingAtMachine;
            return;
        }

        bool took = cookingMachineController.TryTakeItem(actualTake);
        if (!took)
        {
            currentState = CourierState.WaitingAtMachine;
            return;
        }

        currentCarryCount += actualTake;

        if (stackController != null)
            stackController.AddProductStack(actualTake);

        currentState = CourierState.GoingToDesk;
    }

    // 데스크에 1개씩 수프 적재
    // 데스크가 가득 차면 데스크 앞에서 대기
    private void DepositToDesk()
    {
        if (saleDeskController == null)
            return;

        if (currentCarryCount <= 0)
        {
            currentState = CourierState.WaitingAtDesk;
            return;
        }

        if (!saleDeskController.CanAddSoup(1))
        {
            currentState = CourierState.WaitingAtDesk;
            return;
        }

        depositTimer += Time.deltaTime;

        if (depositTimer < depositInterval)
            return;

        depositTimer = 0f;

        bool added = saleDeskController.TryAddSoup(1);
        if (!added)
        {
            currentState = CourierState.WaitingAtDesk;
            return;
        }

        currentCarryCount--;

        if (stackController != null)
            stackController.RemoveProductStack(1);

        if (currentCarryCount <= 0)
            currentState = CourierState.WaitingAtDesk;
    }

    private void MoveToTarget(Transform target)
    {
        if (target == null)
            return;

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    private bool IsArrived(Transform target)
    {
        if (target == null)
            return false;
        return Vector3.Distance(transform.position, target.position) <= arriveDistance;
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isMoving =
            currentState == CourierState.GoingToMachine ||
            currentState == CourierState.GoingToDesk;

        bool hasCarry = currentCarryCount > 0;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(HasCarryHash, hasCarry);
    }
}

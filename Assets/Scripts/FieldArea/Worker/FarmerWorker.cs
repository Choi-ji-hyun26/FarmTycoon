using UnityEngine;

/*
역할
1. 시작점 -> 끝점 방향으로 이동
2. 전진 중 앞에 있는 당근을 찾으면 멈추고 수확
3. harvestInterval마다 1회 타격 / 1 데미지 적용
4. 당근 획득 시 보상 지급 후 짧게 전진
5. 끝점 도달 시 시작점으로 복귀 (복귀 중 수확 X)
6. claim을 사용해 여러 farmer가 같은 당근을 동시에 치지 않게 함
*/
public class FarmerWorker : MonoBehaviour
{
    // 작업 상태 정의
    private enum FarmerState
    {
        Inactive,           // 비활성 상태
        MovingForward,      // 끝점 방향 전진
        Harvesting,             // 수확 중
        Returning,          // 시작점으로 복귀 중
        AdvancingAfterHarvest  // 당근 수확 직후 짧게 강제 전진
    }

    [Header("Route")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Harvesting")]
    [SerializeField] private int hitDamage = 1;
    [SerializeField] private float detectRadius = 1.2f;              // 당근 탐색 범위
    [SerializeField] private float harvestingStopDistance = 0.7f;        // 실제 수확 시작 거리
    [SerializeField] private float harvestInterval = 0.8f;              // 수확 간격
    [SerializeField] private float postHarvestAdvanceDistance = 0.5f;   // 당근 파괴 후 추가 전진 거리
    [SerializeField] private LayerMask carrotLayer;
    [SerializeField, Range(-1f, 1f)] private float forwardDotThreshold = 0.2f;

    [Header("References")]
    [SerializeField] private CookingMachineController cookingMachineController;
    [SerializeField] private Animator animator;

    // 현재 상태
    private FarmerState currentState = FarmerState.Inactive;

    // 현재 수확 대상
    private CarrotNode currentTarget;
    private Collider currentTargetCollider;

    // 수확 타이머
    private float harvestTimer;

    // 파괴 후 강제 전진 시작 위치
    private Vector3 postHarvestAdvanceStartPosition;

    // Animator 파라미터
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int DoHarvestHitHash = Animator.StringToHash("DoHit");

    // 시작 시 시작점 위치로 보정하고 애니메이션 상태를 초기화
    private void Start()
    {
        if (startPoint != null)
            transform.position = startPoint.position;

        RefreshAnimation();
    }

    // 현재 상태에 따라 행동을 갱신
    private void Update()
    {
        switch (currentState)
        {
            case FarmerState.Inactive:
                break;

            case FarmerState.MovingForward:
                UpdateMovingForward();
                break;

            case FarmerState.Harvesting:
                UpdateHarvesting();
                break;

            case FarmerState.Returning:
                UpdateReturning();
                break;

            case FarmerState.AdvancingAfterHarvest:
                UpdateAdvancingAfterHarvest();
                break;
        }
    }

    // farmer 활성화하고 시작점에서 전진 작업을 시작
    public void ActivateWorker()
    {
        if (startPoint != null)
            transform.position = startPoint.position;

        ClearTarget();
        harvestTimer = 0f;
        ChangeState(FarmerState.MovingForward);
    }

    // 전진 중 상태
    // 앞에 수확 가능한 당근이 있으면 claim 후 harvesting 상태로 전환
    // 없으면 endPoint 방향으로 이동
    private void UpdateMovingForward()
    {
        if (TryAcquireForwardCarrot(out CarrotNode carrot, out Collider carrotCollider))
        {
            currentTarget = carrot;
            currentTargetCollider = carrotCollider;
            harvestTimer = 0f;
            ChangeState(FarmerState.Harvesting);
            return;
        }
        MoveToPoint(endPoint);

        if (IsArrived(endPoint))
        {
            ReleaseCurrentTarget();
            ChangeState(FarmerState.Returning);
        }
    }

    // 수확 중 상태
    // harvestInterval마다 실제 1회 수확 판정을 수행
    // 애니메이션은 표현용으로만 트리거
    private void UpdateHarvesting()
    {
        if (!IsValidTarget(currentTarget))
        {
            ReleaseCurrentTarget();
            ChangeState(FarmerState.MovingForward);
            return;
        }

        Vector3 facePoint = currentTargetCollider != null
            ? currentTargetCollider.ClosestPoint(transform.position)
            : currentTarget.transform.position;

        FaceTarget(facePoint);

        float distanceToTarget = currentTargetCollider != null
            ? GetHorizontalDistance(transform.position, currentTargetCollider.ClosestPoint(transform.position))
            : GetHorizontalDistance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > harvestingStopDistance + 0.25f)
        {
            ReleaseCurrentTarget();
            ChangeState(FarmerState.MovingForward);
            return;
        }

        harvestTimer += Time.deltaTime;
        if (harvestTimer < harvestInterval)
            return;

        harvestTimer = 0f;
        PlayHarvestAnimation();
        ExecuteHarvest();
    }

    // 복귀 상태 : 시작점까지 이동하며 복귀 중에는 수확하지 않음
    private void UpdateReturning()
    {
        MoveToPoint(startPoint);

        if (IsArrived(startPoint))
        {
            ReleaseCurrentTarget();
            ChangeState(FarmerState.MovingForward);
        }
    }


    // 당근 파괴 직후 짧게 앞으로 전진하는 상태
    // 즉시 같은 자리에서 다시 당근을 잡는 현상을 줄임
    private void UpdateAdvancingAfterHarvest()
    {
        MoveToPoint(endPoint);

        float movedDistance = GetHorizontalDistance(postHarvestAdvanceStartPosition, transform.position);
        if (movedDistance >= postHarvestAdvanceDistance)
        {
            ChangeState(FarmerState.MovingForward);
            return;
        }

        if (IsArrived(endPoint))
        {
            ChangeState(FarmerState.Returning);
        }
    }

    // 지정한 목표 지점까지 회전하며 이동
    private void MoveToPoint(Transform target)
    {
        if (target == null)
            return;

        Vector3 targetPos = target.position;
        targetPos.y = transform.position.y;

        Vector3 toTarget = targetPos - transform.position;
        toTarget.y = 0f;

        // 목표 방향을 바라보도록 회전
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // 목표 지점까지 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    // 목표 지점 도착 여부를 수평 거리 기준으로 판정
    private bool IsArrived(Transform target)
    {
        if (target == null)
            return false;

        return GetHorizontalDistance(transform.position, target.position) <= arriveDistance;
    }

    // 지정한 월드 위치를 향하도록 회전
    private void FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // 전방의 가까운 당근을 탐색
    // detectRadius 안에서 검색하되 harvestingStopDistance 안쪽이면서
    // 아직 다른 farmer가 claim하지 않은 당근만 선택
    private bool TryAcquireForwardCarrot(out CarrotNode carrot, out Collider carrotCollider)
    {
        carrot = null;
        carrotCollider = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, carrotLayer);

        float nearestSqrDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];
            CarrotNode candidate = col.GetComponentInParent<CarrotNode>();

            // 유효하지 않은 당근 제외
            if (!IsValidTarget(candidate))
                continue;

            Vector3 closestPoint = col.ClosestPoint(transform.position);
            Vector3 toCarrot = closestPoint - transform.position;
            toCarrot.y = 0f;

            if (toCarrot.sqrMagnitude < 0.0001f)
                continue;

            // 너무 뒤쪽이나 옆쪽 당근은 제외
            float dot = Vector3.Dot(transform.forward, toCarrot.normalized);
            if (dot < forwardDotThreshold)
                continue;

            float sqrDistance = toCarrot.sqrMagnitude;

            // 아직 충분히 가깝지 않으면 이동을 계속함
            if (sqrDistance > harvestingStopDistance * harvestingStopDistance)
                continue;

            // 다른 farmer가 선점한 당근은 제외
            if (!candidate.TryClaim(this))
                continue;

            // 가장 가까운 당근 선택
            if (sqrDistance < nearestSqrDistance)
            {
                // 이전 후보를 잠깐 claim했다면 풀어준다
                if (carrot != null && carrot != candidate)
                    carrot.ReleaseClaim(this);

                nearestSqrDistance = sqrDistance;
                carrot = candidate;
                carrotCollider = col;
            }
            else
            {
                // 더 가까운 후보가 아니면 claim 해제
                candidate.ReleaseClaim(this);
            }
        }

        return carrot != null;
    }

    // 현재 타겟 당근이 수확 가능한 상태인지 확인
    private bool IsValidTarget(CarrotNode carrot)
    {
        return carrot != null && !carrot.IsDepleted;
    }


    // 현재 타겟의 claim을 해제하고 타겟 정보를 비움
    private void ReleaseCurrentTarget()
    {
        if (currentTarget != null)
            currentTarget.ReleaseClaim(this);

        currentTarget = null;
        currentTargetCollider = null;
    }


    // 타겟 정보를 비움
    // Activate 직후처럼 claim 해제가 필요 없을 때 사용
    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetCollider = null;
    }

    private void ChangeState(FarmerState nextState)
    {
        currentState = nextState;
        RefreshAnimation();
    }

    private void RefreshAnimation()
    {
        if (animator == null)
            return;

        bool isRunning =
            currentState == FarmerState.MovingForward ||
            currentState == FarmerState.Returning ||
            currentState == FarmerState.AdvancingAfterHarvest;

        animator.SetBool(IsRunningHash, isRunning);
    }

    private void PlayHarvestAnimation()
    {
        if (animator == null)
            return;

        animator.SetTrigger(DoHarvestHitHash);
    }

    // 실제 수확 판정을 수행
    // 데미지 적용, 사운드 재생, 당근 파괴 시 보상 지급과 상태 전환을 담당
    private void ExecuteHarvest()
    {
        if (!IsValidTarget(currentTarget))
            return;

        Vector3 hitPoint = currentTargetCollider != null
            ? currentTargetCollider.ClosestPoint(transform.position)
            : currentTarget.transform.position;

        bool destroyedThisHit = currentTarget.Harvest(hitDamage, hitPoint);

        // 1타당 1회 사운드 재생
        Sfx.PlayAtPoint(SoundId.Harvest, transform.position);

        // 당근이 파괴되면 자원 지급 후 짧게 전진
        if (destroyedThisHit)
        {
            HandleProduction();

            ReleaseCurrentTarget();
            postHarvestAdvanceStartPosition = transform.position;
            ChangeState(FarmerState.AdvancingAfterHarvest);
        }
    }

    private void HandleProduction()
    {
        cookingMachineController.TryAddProductWithAnimation(1, transform.position);
    }

    // y축을 무시한 수평 거리 계산 함수
    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, harvestingStopDistance);
    }
}
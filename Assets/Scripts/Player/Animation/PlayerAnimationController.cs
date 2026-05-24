using UnityEngine;

/*
역할
애니메이션 상태 결정 + Animator 파라미터 적용
- 속도 / 인벤토리 / 존 / 도구 티어 기반으로 상태 계산
- ApplyState()로 Animator에 반영
*/
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    private static readonly int IsCarryingHash   = Animator.StringToHash("IsCarrying");
    private static readonly int IsHarvestingHash  = Animator.StringToHash("IsHarvesting");
    private static readonly int IsHittingHash     = Animator.StringToHash("IsHitting");
    private static readonly int ToolTierHash      = Animator.StringToHash("ToolTier");
    private static readonly int SpeedHash         = Animator.StringToHash("Speed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // 외부에서 매 프레임 호출
    // 상태 결정과 Animator 적용을 한 번에 처리
    public void Tick(
        float speed,
        bool isHarvesting,
        bool isHitting,
        bool isCarrying,
        FarmingToolTier toolTier)
    {
        PlayerAnimState state = ResolveAnimationState(speed, isCarrying);
        ApplyState(state, speed, toolTier, isHarvesting, isHitting);
    }

    // 이동 속도와 적재 여부를 기반으로 애니메이션 상태 결정
    private PlayerAnimState ResolveAnimationState(float speed, bool isCarrying)
    {
        bool isMoving = speed > 0.1f;

        if (isCarrying)
            return isMoving ? PlayerAnimState.CarryRun : PlayerAnimState.CarryIdle;

        return isMoving ? PlayerAnimState.Run : PlayerAnimState.Idle;
    }

    // Animator 파라미터 적용
    private void ApplyState(
        PlayerAnimState newState,
        float speed,
        FarmingToolTier toolTier,
        bool isHarvesting,
        bool isHitting)
    {
        bool isCarrying =
            newState == PlayerAnimState.CarryIdle ||
            newState == PlayerAnimState.CarryRun;

        animator.SetFloat(SpeedHash, speed);
        animator.SetBool(IsCarryingHash,  isCarrying);
        animator.SetBool(IsHarvestingHash, isHarvesting);
        animator.SetBool(IsHittingHash,   isHitting);
        animator.SetInteger(ToolTierHash, (int)toolTier);
    }
}

using System.Collections;
using UnityEngine;

/*
역할
1. 도구 업그레이드 비용 관리
2. 업그레이드 진행도 관리
3. 업그레이드 완료 시 PlayerFarmingToolController에 해금 요청
4. 완료 시 ZoneCostVisual 완료 연출 재생
5. 완료 후 단계 전환 트리거 호출
*/
public class ToolUpgradeController : MonoBehaviour, IMoneyDepositTarget
{
    [Header("References")]
    [SerializeField] private PlayerFarmingToolController farmingToolController;
    [SerializeField] private PlayerInventory playerInventory;        
    [SerializeField] private UpgradeStepTransitionTrigger transitionTrigger;
    [SerializeField] private ZoneCostVisual zoneCostVisual;

    [Header("Upgrade Settings")]
    [SerializeField] private FarmingToolTier targetTier = FarmingToolTier.Rake;
    [SerializeField] private int requiredCost = 10;
    [SerializeField] private float completeDelay = 0.3f;
    [SerializeField] private int expandedInventoryCapacity = 0; // 0이면 확장 없음

    private int currentPaid;
    private bool isCompleted;

    public bool IsCompleted => isCompleted;
    public int RemainingCost => Mathf.Max(0, requiredCost - currentPaid);
    public FarmingToolTier TargetTier => targetTier;

    private void Start()
    {
        RefreshVisual();
    }

    public void DepositMoney(int amount)
    {
        if (isCompleted)
            return;

        if (amount <= 0)
            return;

        int actualDeposit = Mathf.Min(amount, RemainingCost);
        if (actualDeposit <= 0)
            return;

        currentPaid += actualDeposit;

        if (currentPaid > requiredCost)
            currentPaid = requiredCost;

        RefreshVisual();

        if (currentPaid >= requiredCost)
        {
            StartCoroutine(CompleteUpgradeRoutine());
        }
    }

    private IEnumerator CompleteUpgradeRoutine()
    {
        if (isCompleted)
            yield break;

        isCompleted = true;
        currentPaid = requiredCost;

        RefreshVisual();

        if (farmingToolController != null)
            farmingToolController.UnlockTier(targetTier);

        EventBus<ToolUpgradedEvent>.Publish(new ToolUpgradedEvent());

        // 인벤토리 용량 확장 (설정된 경우에만)
        if (playerInventory != null && expandedInventoryCapacity > 0)
        {
            playerInventory.ExpandCapacity(expandedInventoryCapacity);
        }

        if (zoneCostVisual != null)
        {
            zoneCostVisual.PlayCompletedVisual();
        }

        yield return new WaitForSeconds(completeDelay);

        if (transitionTrigger != null)
        {
            transitionTrigger.TriggerTransition();
        }

        UpgradeCompletionTracker.Instance?.NotifyUpgradeCompleted();
    }

    private void RefreshVisual()
    {
        if (zoneCostVisual != null)
        {
            zoneCostVisual.SetCost(currentPaid, requiredCost);
        }
    }
}
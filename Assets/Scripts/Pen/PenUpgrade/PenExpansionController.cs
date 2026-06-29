using System.Collections;
using UnityEngine;

/*
역할
1. 플레이어 돈 지불 처리
2. 완납 시 PenSlotController에 슬롯 해금 요청
3. 완료 시 UpgradeCompletionTracker에 보고
*/
public class PenExpansionController : MonoBehaviour, IMoneyDepositTarget
{
    [Header("References")]
    [SerializeField] private PenSlotController penSlotController;
    [SerializeField] private PenCollectionBox penCollectionBox;
    [SerializeField] private UpgradeStepTransitionTrigger transitionTrigger;
    [SerializeField] private ZoneCostVisual zoneCostVisual;
    [SerializeField] private CustomerDeskQueueManager customerDeskQueueManager;

    [Header("Upgrade Settings")]
    [SerializeField] private int requiredCost = 50;
    [SerializeField] private int slotCountToUnlock = 5;
    [SerializeField] private float completeDelay = 0.3f;
    [SerializeField] private int expandedMilkCapacity = 0;

    private int currentPaid;
    private bool isCompleted;

    public bool IsCompleted => isCompleted;
    public int RemainingCost => Mathf.Max(0, requiredCost - currentPaid);

    public void DepositMoney(int amount)
    {
        if (isCompleted) return;
        if (amount <= 0) return;

        int actual = Mathf.Min(amount, RemainingCost);
        if (actual <= 0) return;

        currentPaid += actual;

        if (currentPaid > requiredCost)
            currentPaid = requiredCost;

        RefreshVisual();

        if (currentPaid >= requiredCost)
            StartCoroutine(CompleteUpgradeRoutine());
    }

    private IEnumerator CompleteUpgradeRoutine()
    {
        if (isCompleted)
            yield break;

        isCompleted = true;
        currentPaid = requiredCost;

        RefreshVisual();

        penSlotController.UnlockSlots(slotCountToUnlock);

        EventBus.Publish(new PenExpandedEvent());

        if (penCollectionBox != null && expandedMilkCapacity > 0)
            penCollectionBox.ExpandCapacity(expandedMilkCapacity);

        if (zoneCostVisual != null)
            zoneCostVisual.PlayCompletedVisual();

        yield return new WaitForSeconds(completeDelay);

        if (transitionTrigger != null)
            transitionTrigger.TriggerTransition();

        if (customerDeskQueueManager != null)
            customerDeskQueueManager.UnlockMilkCustomer();

        UpgradeCompletionTracker.Instance?.NotifyUpgradeCompleted();
    }

    private void RefreshVisual()
    {
        if (zoneCostVisual != null)
            zoneCostVisual.SetCost(currentPaid, requiredCost);
    }
}

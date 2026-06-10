using System.Collections;
using UnityEngine;

/*
역할
1. 2차 확장 완료 시 게임 클리어 트리거
존 활성화는 UpgradeCompletionTracker.OnAllUpgradesCompleted 이벤트를
인스펙터 UnityEvent로 PenExpansionZone2.SetActive(true)에 직접 연결
*/
public class PenFinalExpansionController : MonoBehaviour, IMoneyDepositTarget
{
    [Header("Upgrade Settings")]
    [SerializeField] private int requiredCost = 50;
    [SerializeField] private float completeDelay = 0.3f;
    [SerializeField] private int slotCountToUnlock = 5;
    [SerializeField] private int expandedMilkCapacity = 0;

    [Header("References")]
    [SerializeField] private PenSlotController penSlotController;
    [SerializeField] private PenCollectionBox penCollectionBox;
    [SerializeField] private UpgradeStepTransitionTrigger transitionTrigger;
    [SerializeField] private ZoneCostVisual zoneCostVisual;
    [SerializeField] private GameClearController gameClearController;

    private int currentPaid;
    private bool isCompleted;

    public bool IsCompleted => isCompleted;
    public int RemainingCost => Mathf.Max(0, requiredCost - currentPaid);

    private void Start()
    {
        RefreshVisual();
    }

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

        penSlotController?.UnlockSlots(slotCountToUnlock);

        if (penCollectionBox != null && expandedMilkCapacity > 0)
            penCollectionBox.ExpandCapacity(expandedMilkCapacity);

        if (zoneCostVisual != null)
            zoneCostVisual.PlayCompletedVisual();

        yield return new WaitForSeconds(completeDelay);

        if (transitionTrigger != null)
            transitionTrigger.TriggerTransition();

        if (gameClearController != null)
            gameClearController.TriggerGameClear();
    }

    private void RefreshVisual()
    {
        if (zoneCostVisual != null)
            zoneCostVisual.SetCost(currentPaid, requiredCost);
    }
}

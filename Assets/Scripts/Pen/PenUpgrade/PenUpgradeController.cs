using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 플레이어 돈 지불 처리
2. 완납 시 PenSlotController에 슬롯 해금 요청
3. 두 번째 해금 존 활성화
*/
public class PenUpgradeController : MonoBehaviour, IMoneyDepositTarget
{
    [Header("References")]
    [SerializeField] private PenSlotController penSlotController;
    [SerializeField] private PenCollectionBox penCollectionBox;     // 추가
    [SerializeField] private UpgradeStepTransitionTrigger transitionTrigger;
    [SerializeField] private ZoneCostVisual zoneCostVisual;
    [SerializeField] private CustomerDeskQueueManager customerDeskQueueManager;

    [Header("Upgrade Settings")]
    [SerializeField] private int requiredCost = 50;
    [SerializeField] private int slotCountToUnlock = 5;
    [SerializeField] private float completeDelay = 0.3f;
    [SerializeField] private int expandedMilkCapacity = 0; // 0이면 확장 없음, 2배 원하면 현재값 * 2 입력

    [Header("Game Clear")]
    [SerializeField] private GameClearController gameClearController; // 2번째만 연결

    private int currentPaid;
    private bool isCompleted;

    public bool IsCompleted => isCompleted;
    public int RemainingCost => Mathf.Max(0, requiredCost - currentPaid);

    // 플레이어가 해금 존에서 돈 지불
    // IMoneyDepositTarget 구현
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

    // 비용 완납 시 슬롯 해금 및 존 비활성화
    private IEnumerator CompleteUpgradeRoutine()
    {
        if (isCompleted)  
            yield break;

        isCompleted = true;
        currentPaid = requiredCost;

        RefreshVisual();

        penSlotController.UnlockSlots(slotCountToUnlock);

        // 우유 보관 용량 확장
        if (penCollectionBox != null && expandedMilkCapacity > 0)
            penCollectionBox.ExpandCapacity(expandedMilkCapacity);

        if (zoneCostVisual != null)
            zoneCostVisual.PlayCompletedVisual();

        yield return new WaitForSeconds(completeDelay);

        if (transitionTrigger != null)
        {
            transitionTrigger.TriggerTransition();
        }

        // 첫 번째 펜 해금 시만 우유 손님 활성화
        if (customerDeskQueueManager != null)
            customerDeskQueueManager.UnlockMilkCustomer();

        // 2번째 완료 → 게임 클리어
        if (gameClearController != null)
            gameClearController.TriggerGameClear();
    }

    // 납부 금액 UI 갱신
    private void RefreshVisual()
    {
        if (zoneCostVisual != null)
            zoneCostVisual.SetCost(currentPaid, requiredCost);
    }
}

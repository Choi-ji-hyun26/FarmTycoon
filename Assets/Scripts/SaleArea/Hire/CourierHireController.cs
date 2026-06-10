using System.Collections;
using UnityEngine;
/*
역할
1. 수갑 배달 인력 고용 비용 관리
2. 고용 완료 시 배달 인력 활성화
3. 완료 이벤트 발행
*/
public class CourierHireController : MonoBehaviour, IMoneyDepositTarget
{
    [Header("Settings")]
    [SerializeField] private int requiredCost = 5;
    [SerializeField] private float completeDelay = 0.3f;

    [Header("References")]
    [SerializeField] private CourierWorker courierWorker;
    [SerializeField] private UpgradeStepTransitionTrigger transitionTrigger;
     [SerializeField] private ZoneCostVisual zoneCostVisual;


    private int currentPaid = 0;
    private bool isCompleted = false;

    public bool IsCompleted => isCompleted;
    public int RemainingCost => Mathf.Max(0, requiredCost - currentPaid);

    private void Start()
    {
        RefreshVisual();
    }

    public void DepositMoney(int amount)
    {
        if (isCompleted || amount <= 0)
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

        if (courierWorker != null)
        {
            courierWorker.ActivateWorker();
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

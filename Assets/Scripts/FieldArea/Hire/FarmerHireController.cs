using System.Collections;
using UnityEngine;

/*
역할
1. 마이너 고용 비용 관리
2. 고용 완료 처리 시 마이너 그룹 활성화
3. 완료 시 단계 전환 트리거 호출
*/
public class FarmerHireController : MonoBehaviour, IMoneyDepositTarget
{
    [Header("Hire Settings")]
    [SerializeField] private int requiredCost = 10;
    [SerializeField] private float completeDelay = 0.3f;

    [Header("References")]
    [SerializeField] private FarmerGroupController farmerGroupController;
    [SerializeField] private UpgradeStepTransitionTrigger transitionTrigger;
    [SerializeField] private ZoneCostVisual zoneCostVisual;


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

        if (farmerGroupController != null)
        {
            farmerGroupController.ActivateFarmers();
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
    }

    private void RefreshVisual()
    {
        if (zoneCostVisual != null)
        {
            zoneCostVisual.SetCost(currentPaid, requiredCost);
        }
    }
}
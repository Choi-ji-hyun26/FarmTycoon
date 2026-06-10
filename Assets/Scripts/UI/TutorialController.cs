using UnityEngine;
using TMPro;
using System.Collections;

/*
역할
1. 단계별 튜토리얼 진행
2. 목표 위치에 화살표 표시
3. 상단 텍스트로 목표 안내
4. 조건 달성 시 다음 단계로 진행
*/
public class TutorialController : MonoBehaviour
{
    public enum TutorialStep
    {
        HarvestCarrot,
        DeliverCarrot,
        DeliverSoup,
        Completed
    }

    [Header("UI")]
    [SerializeField] private GameObject tutorialUIRoot;
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private GameObject checkMark;      // V 오브젝트
    [SerializeField] private GameObject arrowIndicator; // 목표 위 화살표

    [Header("Arrow Targets")]
    [SerializeField] private Transform harvestTarget;   // 밭 위치
    [SerializeField] private Transform cookingTarget;   // 쿠킹존 위치
    [SerializeField] private Transform saleTarget;      // 판매존 위치

    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private CookingMachineController cookingMachineController;

    [Header("Settings")]
    [SerializeField] private float checkMarkDuration = 1f; // V 표시 시간
    [SerializeField] private float arrowBobSpeed = 2f;     // 화살표 위아래 움직임
    [SerializeField] private float arrowBobAmount = 0.2f;

    private TutorialStep currentStep = TutorialStep.HarvestCarrot;
    private Vector3 arrowBasePosition;
    private bool isTransitioning;
    private bool hadSoup = false; // 수프를 한 번이라도 가진 적이 있는지 체크

    private static readonly string[] GuideTexts =
    {
        "Harvest \nthe carrots!",
        "Put the carrots \ninto the pot!",
        "Place the soup \non the counter!"
    };

    private void Start()
    {
        checkMark?.SetActive(false);
        ShowStep(currentStep);
    }

    private void Update()
    {
        if (currentStep == TutorialStep.Completed) return;
        if (isTransitioning) return;

        AnimateArrow();
        CheckStepCondition();
    }

    // 현재 단계 UI 및 화살표 세팅
    private void ShowStep(TutorialStep step)
    {
        tutorialUIRoot?.SetActive(step != TutorialStep.Completed);

        if (step == TutorialStep.Completed) return;

        guideText.text = GuideTexts[(int)step];

        Transform target = GetTargetForStep(step);
        if (target != null && arrowIndicator != null)
        {
            arrowIndicator.SetActive(true);
            arrowBasePosition = target.position + Vector3.up * 1.5f;
            arrowIndicator.transform.position = arrowBasePosition;
        }
    }

    // 화살표 위아래 부드럽게 움직임
    private void AnimateArrow()
    {
        if (arrowIndicator == null || !arrowIndicator.activeSelf) return;

        float bobOffset = Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmount;
        arrowIndicator.transform.position = arrowBasePosition + Vector3.up * bobOffset;
    }

    // 단계별 완료 조건 체크
    private void CheckStepCondition()
    {
        bool conditionMet = currentStep switch
        {
            TutorialStep.HarvestCarrot  => inventory.CarrotCount >= 1,
            TutorialStep.DeliverCarrot  => cookingMachineController.StoredCarrotCount >= 1,
            TutorialStep.DeliverSoup    => CheckSoupDelivered(),
            _ => false
        };

        if (conditionMet)
            StartCoroutine(CompleteStepRoutine());
    }

    // 수프를 한 번 이상 보유했다가 0이 되면 데스크에 올린 것으로 판정
    private bool CheckSoupDelivered()
    {
        if(inventory.SoupCount >= 1)
            hadSoup = true;

        return hadSoup && inventory.SoupCount == 0;
    }

    // V 표시 후 다음 단계로 전환
    private IEnumerator CompleteStepRoutine()
    {
        isTransitioning = true;

        arrowIndicator?.SetActive(false);
        guideText.gameObject.SetActive(false);
        checkMark?.SetActive(true);

        yield return new WaitForSeconds(checkMarkDuration);

        checkMark?.SetActive(false);
        guideText.gameObject.SetActive(true);

        currentStep++;
        isTransitioning = false;

        ShowStep(currentStep);
    }

    private Transform GetTargetForStep(TutorialStep step) => step switch
    {
        TutorialStep.HarvestCarrot => harvestTarget,
        TutorialStep.DeliverCarrot => cookingTarget,
        TutorialStep.DeliverSoup   => saleTarget,
        _ => null
    };
}
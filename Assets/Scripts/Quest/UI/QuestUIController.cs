using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using VContainer;

/// <summary>
/// 우측 고정 퀘스트 사이드 패널 UI
/// QuestManager로부터 상태 변경 알림을 받아 갱신
/// 플레이어 클릭 시 Claimable 상태면 보상 수령
/// </summary>
public class QuestUIController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button claimButton;

    [Header("Info")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text expRewardText;

    [Header("Complete")]
    [SerializeField] private GameObject checkMarkImage;

    [Header("DOTween Settings")]
    [SerializeField] private float claimScaleDuration = 0.2f;
    [SerializeField] private float shakeStrength = 10f;
    [SerializeField] private float shakeDuration = 0.3f;

    private QuestManager _questManager;

    [Inject]
    public void Construct(QuestManager questManager)
    {
        _questManager = questManager;
    }

    private void Start()
    {
        claimButton.onClick.AddListener(OnClaimButtonClicked);
        _questManager.OnQuestChanged += Refresh;
        _questManager.OnQuestClaimable += RefreshClaimable;
        _questManager.OnProgressChanged += RefreshProgress;

        Refresh(_questManager.CurrentQuest);
    }

    private void OnDestroy()
    {
        if (_questManager != null)
        {
            _questManager.OnQuestChanged -= Refresh;
            _questManager.OnQuestClaimable -= RefreshClaimable;
            _questManager.OnProgressChanged -= RefreshProgress;
        }
        claimButton.onClick.RemoveListener(OnClaimButtonClicked);
    }

    // 퀘스트 전환 시 전체 갱신
    private void Refresh(QuestInstance quest)
    {
        if (quest == null)
        {
            panelRoot.SetActive(false);
            return;
        }

        panelRoot.SetActive(true);
        checkMarkImage.SetActive(false);
        claimButton.interactable = false;

        titleText.text = quest.Data.Title;
        progressText.text = $"{quest.CurrentValue} / {quest.TargetValue}";

        if (quest.Data.Reward is ExpReward expReward)
            expRewardText.text = $"보상: {expReward.ExpAmount} EXP";
        else
            expRewardText.text = string.Empty;
    }

    // Claimable 상태 전환 시
    private void RefreshClaimable(QuestInstance quest)
    {
        if (quest == null) return;

        progressText.text = $"{quest.CurrentValue} / {quest.TargetValue}";
        claimButton.interactable = true;
        checkMarkImage.SetActive(true);

        panelRoot.transform
            .DOShakePosition(shakeDuration, shakeStrength)
            .SetUpdate(true);
    }

    // 진행도만 갱신
    private void RefreshProgress(QuestInstance quest)
    {
        if (quest == null) return;
        progressText.text = $"{quest.CurrentValue} / {quest.TargetValue}";
    }

    private void OnClaimButtonClicked()
    {
        if (_questManager.CurrentQuest == null) return;
        if (_questManager.CurrentQuest.State != QuestState.Claimable) return;

        PlayClaimAnimation(() =>
        {
            checkMarkImage.SetActive(false);
            _questManager.ClaimCurrentQuest();
        });
    }

    private void PlayClaimAnimation(System.Action onComplete)
    {
        panelRoot.transform
            .DOScale(0.9f, claimScaleDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                panelRoot.transform
                    .DOScale(1f, claimScaleDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => onComplete?.Invoke());
            });
    }
}

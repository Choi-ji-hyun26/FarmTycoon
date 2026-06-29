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
    [SerializeField] private TMP_Text progressText;   // "4 / 5"
    [SerializeField] private Slider progressBar;

    [Header("Reward")]
    [SerializeField] private TMP_Text expRewardText;  // "+ 10 EXP"

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

        // 초기 상태 반영
        Refresh(_questManager.CurrentQuest);
    }

    private void OnDestroy()
    {
        if (_questManager != null)
            _questManager.OnQuestChanged -= Refresh;

        claimButton.onClick.RemoveListener(OnClaimButtonClicked);
    }

    private void Refresh(QuestInstance quest)
    {
        if (quest == null)
        {
            panelRoot.SetActive(false);
            return;
        }

        panelRoot.SetActive(true);

        titleText.text = quest.Data.Title;
        progressText.text = $"{quest.CurrentValue} / {quest.TargetValue}";
        progressBar.value = (float)quest.CurrentValue / quest.TargetValue;

        // 보상 EXP 표시
        if (quest.Data.Reward is ExpReward expReward)
            expRewardText.text = $"+ {expReward.ExpAmount} EXP";
        else
            expRewardText.text = string.Empty;

        // Claimable 상태일 때 버튼 활성화
        claimButton.interactable = quest.State == QuestState.Claimable;

        if (quest.State == QuestState.Claimable)
            PlayClaimableEffect();
    }

    private void OnClaimButtonClicked()
    {
        if (_questManager.CurrentQuest == null) return;
        if (_questManager.CurrentQuest.State != QuestState.Claimable) return;

        PlayClaimAnimation(() =>
        {
            _questManager.ClaimCurrentQuest();
        });
    }

    // 완료 조건 달성 시 패널 흔들림 연출
    private void PlayClaimableEffect()
    {
        panelRoot.transform
            .DOShakePosition(shakeDuration, shakeStrength)
            .SetUpdate(true);
    }

    // 수령 클릭 시 축소 → 확대 후 콜백
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

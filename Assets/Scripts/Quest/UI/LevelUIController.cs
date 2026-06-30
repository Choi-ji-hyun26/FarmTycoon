using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using VContainer;

/// <summary>
/// 좌측 상단 레벨 UI
/// 별 모양 EXP 게이지(아래→위 채움)와 레벨 숫자 표시
/// 퀘스트 보상 수령 시 별 아이콘이 퀘스트 패널에서 게이지로 날아가는 연출 담당
///
/// 데이터(ExpManager)는 보상 수령 즉시 갱신되고,
/// 별 채우기는 아이콘 도착 시점에 연출로 반영 (A 방식)
/// </summary>
public class LevelUIController : MonoBehaviour
{
    [Header("Star Gauge")]
    [SerializeField] private Image starFill;          // Filled, Vertical, Bottom
    [SerializeField] private TMP_Text levelText;

    [Header("Flying Star Icon")]
    [SerializeField] private Image flyingStarIcon;     // 평소 비활성, 작은 별 아이콘
    [SerializeField] private RectTransform starTarget; // 아이콘이 도착할 별 위치

    [Header("Animation")]
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float fillDuration = 0.4f;
    [SerializeField] private Ease flyEase = Ease.InQuad;

    private ExpManager _expManager;

    [Inject]
    public void Construct(ExpManager expManager)
    {
        _expManager = expManager;
    }

    private void Start()
    {
        flyingStarIcon.gameObject.SetActive(false);
        RefreshImmediate();
    }

    // 현재 상태를 즉시 반영 (연출 없이)
    private void RefreshImmediate()
    {
        levelText.text = _expManager.CurrentLevel.ToString();
        starFill.fillAmount = (float)_expManager.CurrentLevelExp / _expManager.ExpPerLevel;
    }

    /// <summary>
    /// 퀘스트 보상 수령 시 호출
    /// fromPanel 위치에서 별 아이콘이 게이지로 날아간 뒤
    /// 별 게이지를 현재 ExpManager 상태로 채움
    /// </summary>
    public void PlayExpGainEffect(RectTransform fromPanel)
    {
        flyingStarIcon.gameObject.SetActive(true);
        flyingStarIcon.transform.position = fromPanel.position;
        flyingStarIcon.transform.localScale = Vector3.one;

        flyingStarIcon.transform
            .DOMove(starTarget.position, flyDuration)
            .SetEase(flyEase)
            .OnComplete(() =>
            {
                flyingStarIcon.gameObject.SetActive(false);
                ApplyExpToGauge();
            });
    }

    // 아이콘 도착 후 별 게이지 채우기 + 레벨업 처리
    private void ApplyExpToGauge()
    {
        int targetLevel = _expManager.CurrentLevel;
        float targetFill = (float)_expManager.CurrentLevelExp / _expManager.ExpPerLevel;

        bool leveledUp = targetLevel > int.Parse(levelText.text);

        if (leveledUp)
        {
            starFill.DOFillAmount(1f, fillDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    levelText.text = targetLevel.ToString();
                    starFill.fillAmount = 0f;
                    starFill.DOFillAmount(targetFill, fillDuration)
                        .SetEase(Ease.OutQuad);
                });
        }
        else
        {
            starFill.DOFillAmount(targetFill, fillDuration)
                .SetEase(Ease.OutQuad);
        }
    }
}

using UnityEngine;
using DG.Tweening;

/// <summary>
/// 퀘스트 진행 방향 화살표 연출
/// QuestManager로부터 현재 퀘스트 arrowTargetId를 받아
/// QuestArrowRegistry에서 타겟 Transform을 찾아 화살표 표시
/// arrowTargetId가 비어있으면 화살표 숨김
/// </summary>
public class QuestArrowDirector : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobDuration = 0.6f;

    private QuestManager _questManager;
    private Tween _bobTween;

    private void Start()
    {
        // QuestManager는 VContainer 미주입 — 직접 참조 대신 이벤트로 연결
        // GameLifetimeScope에서 QuestArrowDirector를 등록하거나
        // QuestManager.OnQuestChanged를 늦게 구독하는 방식 모두 가능
        // 여기서는 QuestManager 싱글톤 없이 이벤트 직접 구독
    }

    /// <summary>
    /// GameLifetimeScope에서 주입 또는 Start 이후 외부에서 초기화
    /// </summary>
    public void Initialize(QuestManager questManager)
    {
        _questManager = questManager;
        _questManager.OnQuestChanged += OnQuestChanged;
        OnQuestChanged(_questManager.CurrentQuest);
    }

    private void OnDestroy()
    {
        _bobTween?.Kill();
        if (_questManager != null)
            _questManager.OnQuestChanged -= OnQuestChanged;
    }

    private void OnQuestChanged(QuestInstance quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.Data.ArrowTargetId))
        {
            HideArrow();
            return;
        }

        if (QuestArrowRegistry.Instance == null) return;

        if (QuestArrowRegistry.Instance.TryGetTarget(quest.Data.ArrowTargetId, out Transform target))
            ShowArrow(target);
        else
            HideArrow();
    }

    private void ShowArrow(Transform target)
    {
        arrowObject.SetActive(true);
        arrowObject.transform.position = target.position + Vector3.up * 1.5f;

        _bobTween?.Kill();
        _bobTween = arrowObject.transform
            .DOLocalMoveY(arrowObject.transform.localPosition.y + bobHeight, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void HideArrow()
    {
        _bobTween?.Kill();
        arrowObject.SetActive(false);
    }
}

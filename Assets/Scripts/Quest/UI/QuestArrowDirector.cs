using UnityEngine;
using System.Collections;
using DG.Tweening;
using VContainer;

/// <summary>
/// 퀘스트 진행 방향 화살표 연출
/// 퀘스트 시작 시 화살표 표시
/// 첫 진행(CurrentValue > 0) 시 화살표 숨김
/// </summary>
public class QuestArrowDirector : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobDuration = 0.6f;

    private QuestManager _questManager;
    private Tween _bobTween;
    private Coroutine _retryCoroutine;

    [Inject]
    public void Construct(QuestManager questManager)
    {
        _questManager = questManager;
        _questManager.OnQuestChanged += OnQuestChanged;
        _questManager.OnProgressChanged += OnProgressChanged;
    }

    private void OnDestroy()
    {
        _bobTween?.Kill();
        if (_retryCoroutine != null)
            StopCoroutine(_retryCoroutine);
        if (_questManager != null)
        {
            _questManager.OnQuestChanged -= OnQuestChanged;
            _questManager.OnProgressChanged -= OnProgressChanged;
        }
    }

    private void Start()
    {
        if (_questManager?.CurrentQuest != null)
            OnQuestChanged(_questManager.CurrentQuest);
    }

    private void OnQuestChanged(QuestInstance quest)
    {
        if (_retryCoroutine != null)
        {
            StopCoroutine(_retryCoroutine);
            _retryCoroutine = null;
        }

        if (quest == null || string.IsNullOrEmpty(quest.Data.ArrowTargetId))
        {
            HideArrow();
            return;
        }

        _retryCoroutine = StartCoroutine(TryShowArrowWithRetry(quest.Data.ArrowTargetId));
    }

    private void OnProgressChanged(QuestInstance quest)
    {
        // 첫 진행 시 화살표 숨김
        if (quest != null && quest.CurrentValue > 0)
            HideArrow();
    }

    private IEnumerator TryShowArrowWithRetry(string targetId)
    {
        float elapsed = 0f;
        float timeout = 2f;

        while (elapsed < timeout)
        {
            if (QuestArrowRegistry.Instance != null &&
                QuestArrowRegistry.Instance.TryGetTarget(targetId, out Transform target))
            {
                ShowArrow(target);
                yield break;
            }

            yield return null;
            elapsed += Time.deltaTime;
        }

        Debug.LogWarning($"QuestArrowDirector: '{targetId}' 타겟을 찾지 못했습니다.");
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

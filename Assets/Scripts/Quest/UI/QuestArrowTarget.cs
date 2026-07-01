using UnityEngine;

/// <summary>
/// 화살표 타겟으로 사용될 Zone에 부착
/// Start()에서 QuestArrowRegistry에 자신을 등록
/// arrowId는 QuestData.arrowTargetId와 일치해야 함
/// </summary>
public class QuestArrowTarget : MonoBehaviour
{
    [SerializeField] private string arrowId;

    private void Start()
    {
        if (QuestArrowRegistry.Instance == null)
        {
            Debug.LogWarning("QuestArrowRegistry instance not found.");
            return;
        }
        QuestArrowRegistry.Instance.Register(arrowId, transform);
    }

    private void OnDestroy()
    {
        if (QuestArrowRegistry.Instance != null)
            QuestArrowRegistry.Instance.Unregister(arrowId);
    }
}

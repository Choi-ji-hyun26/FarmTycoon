using UnityEngine;

/// <summary>
/// 퀘스트 정의 데이터
/// ScriptableObject로 생성해서 QuestManager에 순서대로 등록
///
/// COUNT 퀘스트: targetEvent + targetValue 사용
/// ACTION 퀘스트: targetEvent + 식별 필드(targetToolTier / targetPenStage) 사용
///   - FarmerHired, CourierHired는 대상이 하나뿐이라 식별 필드 불필요
/// </summary>
[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string questId;
    [SerializeField] private string title;

    [Header("Condition")]
    [SerializeField] private QuestEventType targetEvent;

    [Tooltip("COUNT 타입에서만 사용 (당근 5개 등). ACTION 타입은 무시됨")]
    [SerializeField] private int targetValue = 1;

    [Header("Action Identifier")]
    [Tooltip("ToolUpgraded ACTION 퀘스트에서 어떤 도구 단계를 요구하는지")]
    [SerializeField] private FarmingToolTier targetToolTier = FarmingToolTier.None;

    [Tooltip("PenExpanded ACTION 퀘스트에서 몇 차 확장을 요구하는지")]
    [SerializeField] private PenExpansionStage targetPenStage = PenExpansionStage.None;

    [Header("Reward")]
    [SerializeField] private QuestReward reward;

    [Header("Arrow")]
    [Tooltip("빈 문자열이면 화살표 미표시. QuestArrowRegistry에 등록된 ID와 일치해야 함")]
    [SerializeField] private string arrowTargetId;

    public string QuestId             => questId;
    public string Title               => title;
    public QuestEventType TargetEvent => targetEvent;
    public int TargetValue            => targetValue;
    public FarmingToolTier TargetToolTier => targetToolTier;
    public PenExpansionStage TargetPenStage => targetPenStage;
    public QuestReward Reward         => reward;
    public string ArrowTargetId       => arrowTargetId;

    public QuestEventCategory Category => QuestEventCategoryMap.GetCategory(targetEvent);
}

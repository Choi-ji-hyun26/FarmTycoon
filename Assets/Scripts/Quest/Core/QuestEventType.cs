using System.Collections.Generic;

/// <summary>
/// 퀘스트 완료 조건으로 감지할 이벤트 타입
/// QuestData.targetEvent에서 사용
/// </summary>
public enum QuestEventType
{
    CarrotHarvested,    // 당근 수확
    SoupProduced,       // 수프 생산
    SoupSold,           // 수프 판매
    MilkSold,           // 우유 판매
    CustomerServed,     // 손님 응대
    ToolUpgraded,       // 도구 업그레이드
    FarmerHired,        // 농부 고용
    CourierHired,       // 배달원 고용
    PenExpanded,        // 동물 우리 확장
}

/// <summary>
/// 퀘스트 진행 정책 분류
/// COUNT  : 퀘스트 시작 후부터 카운트 (이전 진행분 제외)
/// ACTION : 퀘스트 시작 이전 완료분도 인정 (절대값 비교)
/// </summary>
public enum QuestEventCategory
{
    Count,
    Action,
}

/// <summary>
/// QuestEventType → QuestEventCategory 매핑
/// 정책이 이벤트 타입에 종속되므로 중앙에서 정의
/// </summary>
public static class QuestEventCategoryMap
{
    private static readonly HashSet<QuestEventType> ActionTypes = new()
    {
        QuestEventType.ToolUpgraded,
        QuestEventType.FarmerHired,
        QuestEventType.CourierHired,
        QuestEventType.PenExpanded,
    };

    public static QuestEventCategory GetCategory(QuestEventType type)
    {
        return ActionTypes.Contains(type)
            ? QuestEventCategory.Action
            : QuestEventCategory.Count;
    }
}

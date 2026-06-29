/// <summary>
/// 퀘스트 진행 상태
/// </summary>
public enum QuestState
{
    Active,     // 진행 중
    Claimable,  // 완료 조건 충족, 클릭 대기
    Completed   // 수령 완료
}

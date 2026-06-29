/// <summary>
/// 퀘스트 보상 지급에 필요한 시스템들을 묶은 컨텍스트
/// ScriptableObject(QuestReward)는 DI 주입을 받을 수 없으므로
/// 보상 지급 시점에 QuestManager가 이 컨텍스트를 전달
///
/// 새 보상 타입이 다른 시스템을 필요로 하면 여기에 추가
/// </summary>
public interface IRewardContext
{
    ExpManager ExpManager { get; }
}

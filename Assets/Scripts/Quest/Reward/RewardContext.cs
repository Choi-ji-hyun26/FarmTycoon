/// <summary>
/// IRewardContext 기본 구현
/// QuestManager가 주입받은 시스템들을 담아 보상 지급 시 전달
/// </summary>
public class RewardContext : IRewardContext
{
    public ExpManager ExpManager { get; }

    public RewardContext(ExpManager expManager)
    {
        ExpManager = expManager;
    }
}

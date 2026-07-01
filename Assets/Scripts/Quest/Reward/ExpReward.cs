using UnityEngine;

/// <summary>
/// 경험치 보상
/// QuestData에서 ScriptableObject로 생성해서 할당
/// 지급은 IRewardContext의 ExpManager를 통해 수행
/// </summary>
[CreateAssetMenu(fileName = "ExpReward", menuName = "Quest/Reward/ExpReward")]
public class ExpReward : QuestReward
{
    [SerializeField] private int expAmount = 10;
    public int ExpAmount => expAmount;

    public override void Grant(IRewardContext context)
    {
        if (context?.ExpManager == null)
        {
            Debug.LogWarning("ExpReward: ExpManager가 컨텍스트에 없습니다.");
            return;
        }
        context.ExpManager.AddExp(expAmount);
    }
}

using UnityEngine;

/// <summary>
/// 퀘스트 보상 기반 추상 클래스
/// 새로운 보상 타입 추가 시 이 클래스를 상속
/// 지급에 필요한 시스템은 IRewardContext로 주입받음
/// </summary>
public abstract class QuestReward : ScriptableObject
{
    public abstract void Grant(IRewardContext context);
}

using UnityEngine;

[CreateAssetMenu(fileName = "ExpReward", menuName = "Quest/Reward/ExpReward")]
public class ExpReward : QuestReward
{
    [SerializeField] private int expAmount = 10;
    public int ExpAmount => expAmount;

    public override void Grant()
    {
        if (ExpManager.Instance == null)
        {
            Debug.LogWarning("ExpManager instance not found.");
            return;
        }
        ExpManager.Instance.AddExp(expAmount);
    }
}
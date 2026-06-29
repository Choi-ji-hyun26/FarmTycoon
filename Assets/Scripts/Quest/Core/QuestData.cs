using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string questId;
    [SerializeField] private string title;

    [Header("Condition")]
    [SerializeField] private QuestEventType targetEvent;
    [SerializeField] private int targetValue = 1;

    [Header("Reward")]
    [SerializeField] private QuestReward reward;

    [Header("Arrow")]
    [Tooltip("빈 문자열이면 화살표 미표시, QuestArrowRegistry에 등록된 ID와 일치해야 함")]
    [SerializeField] private string arrowTargetId;

    public string QuestId             => questId;
    public string Title               => title;
    public QuestEventType TargetEvent => targetEvent;
    public int TargetValue            => targetValue;
    public QuestReward Reward         => reward;
    public string ArrowTargetId       => arrowTargetId;
}
using System;

/// <summary>
/// 퀘스트 런타임 상태
/// 이벤트를 직접 구독하지 않고 QuestProgressTracker로부터 진행도를 갱신받음
/// 진행도/상태 계산만 책임 (단일 책임)
/// </summary>
public class QuestInstance
{
    public QuestData Data { get; private set; }
    public int CurrentValue { get; private set; }
    public int TargetValue { get; private set; }
    public QuestState State { get; private set; }

    // 상태 전환 시 (Active→Claimable, Claimable→Completed)
    public event Action OnStateChanged;

    // 진행도 변경 시
    public event Action OnProgressChanged;

    public QuestInstance(QuestData data)
    {
        Data = data;
        TargetValue = data.TargetValue;
        CurrentValue = 0;
        State = QuestState.Active;
    }

    /// <summary>
    /// QuestProgressTracker가 현재 진행도를 전달
    /// COUNT: 퀘스트 시작 이후 누적분, ACTION: 절대 완료 횟수
    /// 이미 baseValue가 차감된 값이 들어옴
    /// </summary>
    public void UpdateProgress(int newValue)
    {
        if (State != QuestState.Active) return;

        int clamped = newValue;
        if (clamped > TargetValue)
            clamped = TargetValue;

        if (clamped == CurrentValue) return;

        CurrentValue = clamped;
        OnProgressChanged?.Invoke();

        if (CurrentValue >= TargetValue)
        {
            State = QuestState.Claimable;
            OnStateChanged?.Invoke();
        }
    }

    // 플레이어가 패널 클릭 시 호출
    public void Claim()
    {
        if (State != QuestState.Claimable) return;

        Data.Reward?.Grant();
        State = QuestState.Completed;
        OnStateChanged?.Invoke();
    }
}

using System;

/// <summary>
/// 퀘스트 런타임 상태
/// QuestData를 기반으로 생성되며 진행도/상태를 관리
/// </summary>
public class QuestInstance
{
    public QuestData Data { get; private set; }
    public int CurrentValue { get; private set; }
    public int TargetValue { get; private set; }
    public QuestState State { get; private set; }

    // CustomerServed 퀘스트 시작 시점 기준값
    private int _baseValue;

    public event Action OnStateChanged;

    public QuestInstance(QuestData data, int baseValue = 0)
    {
        Data = data;
        TargetValue = data.TargetValue;
        CurrentValue = 0;
        State = QuestState.Active;
        _baseValue = baseValue;
    }

    // EventBus 콜백 — 이벤트 타입별 처리
    public void OnCarrotHarvested(CarrotHarvestedEvent e)  => Accumulate(e.count);
    public void OnSoupProduced(SoupProducedEvent e)        => Accumulate(e.count);
    public void OnSoupSold(SoupSoldEvent e)                => Accumulate(e.count);
    public void OnMilkSold(MilkSoldEvent e)                => Accumulate(e.count);
    public void OnToolUpgraded(ToolUpgradedEvent e)        => Accumulate(1);
    public void OnFarmerHired(FarmerHiredEvent e)          => Accumulate(1);
    public void OnCourierHired(CourierHiredEvent e)        => Accumulate(1);
    public void OnPenExpanded(PenExpandedEvent e)          => Accumulate(1);

    // CustomerServed는 누적값 기반 — 퀘스트 시작 이전 손님 수 제외
    public void OnCustomerServed(CustomerServedEvent e)
    {
        if (State != QuestState.Active) return;
        CurrentValue = e.totalCount - _baseValue;
        CheckClaimable();
    }

    private void Accumulate(int amount)
    {
        if (State != QuestState.Active) return;
        CurrentValue += amount;
        CheckClaimable();
    }

    private void CheckClaimable()
    {
        if (State != QuestState.Active) return;
        if (CurrentValue < TargetValue) return;

        CurrentValue = TargetValue; // 초과 방지
        State = QuestState.Claimable;
        OnStateChanged?.Invoke();
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

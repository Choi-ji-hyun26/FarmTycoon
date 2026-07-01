using System.Collections.Generic;
using VContainer.Unity;

/// <summary>
/// 퀘스트 시스템의 유일한 이벤트 구독자
/// 게임 시작부터 모든 퀘스트 이벤트를 항상 구독해 진행 상태를 기록
///
/// COUNT  : 타입별 누적 카운트 유지
/// ACTION : 완료된 대상을 기록 (도구 tier, 우리 stage, 고용 여부)
///
/// 자유 진행 게임에서 퀘스트가 주어지기 전 수행한 행동을
/// 추적하기 위해 항상 집계. 퀘스트 시작 시 현재 기록으로 충족 여부 즉시 판정
/// </summary>
public class QuestProgressTracker : IStartable, System.IDisposable
{
    // COUNT 누적
    private readonly Dictionary<QuestEventType, int> _countProgress = new();

    // ACTION 완료 기록
    private readonly HashSet<FarmingToolTier> _completedToolTiers = new();
    private readonly HashSet<PenExpansionStage> _completedPenStages = new();
    private bool _farmerHired;
    private bool _courierHired;

    // 현재 진행 중인 퀘스트
    private QuestInstance _activeQuest;
    private int _activeCountBase; // COUNT 퀘스트 시작 시점 누적값

    public void Start()
    {
        EventBus.Subscribe<CarrotHarvestedEvent>(OnCarrotHarvested);
        EventBus.Subscribe<SoupProducedEvent>(OnSoupProduced);
        EventBus.Subscribe<SoupSoldEvent>(OnSoupSold);
        EventBus.Subscribe<MilkSoldEvent>(OnMilkSold);
        EventBus.Subscribe<CustomerServedEvent>(OnCustomerServed);
        EventBus.Subscribe<ToolUpgradedEvent>(OnToolUpgraded);
        EventBus.Subscribe<FarmerHiredEvent>(OnFarmerHired);
        EventBus.Subscribe<CourierHiredEvent>(OnCourierHired);
        EventBus.Subscribe<PenExpandedEvent>(OnPenExpanded);
    }

    /// <summary>
    /// QuestManager가 새 퀘스트 시작 시 호출
    /// 시작 시점에 현재 기록으로 진행도/충족 여부를 즉시 통보
    /// </summary>
    public void RegisterActiveQuest(QuestInstance quest)
    {
        _activeQuest = quest;
        if (quest == null) return;

        var data = quest.Data;

        if (data.Category == QuestEventCategory.Count)
        {
            // COUNT: 시작 시점 누적값을 기준으로 차감
            _activeCountBase = GetCount(data.TargetEvent);
        }

        NotifyActiveQuest();
    }

    public void UnregisterActiveQuest()
    {
        _activeQuest = null;
    }

    // ---- COUNT ----

    private int GetCount(QuestEventType type)
        => _countProgress.TryGetValue(type, out var v) ? v : 0;

    private void AddCount(QuestEventType type, int amount)
    {
        _countProgress.TryGetValue(type, out var current);
        _countProgress[type] = current + amount;
        NotifyIfActive(type);
    }

    private void OnCarrotHarvested(CarrotHarvestedEvent e) => AddCount(QuestEventType.CarrotHarvested, e.count);
    private void OnSoupProduced(SoupProducedEvent e)       => AddCount(QuestEventType.SoupProduced, e.count);
    private void OnSoupSold(SoupSoldEvent e)               => AddCount(QuestEventType.SoupSold, e.count);
    private void OnMilkSold(MilkSoldEvent e)               => AddCount(QuestEventType.MilkSold, e.count);

    private void OnCustomerServed(CustomerServedEvent e)
    {
        // 누적 총량으로 발행되므로 절대값 저장
        _countProgress[QuestEventType.CustomerServed] = e.totalCount;
        NotifyIfActive(QuestEventType.CustomerServed);
    }

    // ---- ACTION ----

    private void OnToolUpgraded(ToolUpgradedEvent e)
    {
        _completedToolTiers.Add(e.tier);
        NotifyIfActive(QuestEventType.ToolUpgraded);
    }

    private void OnPenExpanded(PenExpandedEvent e)
    {
        _completedPenStages.Add(e.stage);
        NotifyIfActive(QuestEventType.PenExpanded);
    }

    private void OnFarmerHired(FarmerHiredEvent e)
    {
        _farmerHired = true;
        NotifyIfActive(QuestEventType.FarmerHired);
    }

    private void OnCourierHired(CourierHiredEvent e)
    {
        _courierHired = true;
        NotifyIfActive(QuestEventType.CourierHired);
    }

    // ---- 통보 ----

    private void NotifyIfActive(QuestEventType type)
    {
        if (_activeQuest != null && _activeQuest.Data.TargetEvent == type)
            NotifyActiveQuest();
    }

    private void NotifyActiveQuest()
    {
        if (_activeQuest == null) return;

        var data = _activeQuest.Data;

        if (data.Category == QuestEventCategory.Count)
        {
            int progress = GetCount(data.TargetEvent) - _activeCountBase;
            _activeQuest.UpdateProgress(progress);
        }
        else
        {
            // ACTION: 충족 시 목표치(1)로, 아니면 0
            bool satisfied = IsActionSatisfied(data);
            _activeQuest.UpdateProgress(satisfied ? data.TargetValue : 0);
        }
    }

    private bool IsActionSatisfied(QuestData data)
    {
        switch (data.TargetEvent)
        {
            case QuestEventType.ToolUpgraded:
                return _completedToolTiers.Contains(data.TargetToolTier);
            case QuestEventType.PenExpanded:
                return _completedPenStages.Contains(data.TargetPenStage);
            case QuestEventType.FarmerHired:
                return _farmerHired;
            case QuestEventType.CourierHired:
                return _courierHired;
            default:
                return false;
        }
    }

    public void Dispose()
    {
        EventBus.Unsubscribe<CarrotHarvestedEvent>(OnCarrotHarvested);
        EventBus.Unsubscribe<SoupProducedEvent>(OnSoupProduced);
        EventBus.Unsubscribe<SoupSoldEvent>(OnSoupSold);
        EventBus.Unsubscribe<MilkSoldEvent>(OnMilkSold);
        EventBus.Unsubscribe<CustomerServedEvent>(OnCustomerServed);
        EventBus.Unsubscribe<ToolUpgradedEvent>(OnToolUpgraded);
        EventBus.Unsubscribe<FarmerHiredEvent>(OnFarmerHired);
        EventBus.Unsubscribe<CourierHiredEvent>(OnCourierHired);
        EventBus.Unsubscribe<PenExpandedEvent>(OnPenExpanded);
    }
}

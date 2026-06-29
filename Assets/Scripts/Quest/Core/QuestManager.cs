using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 퀘스트 진행 전체 관리
/// VContainer로 등록, 싱글톤 아님
/// 퀘스트 순서대로 1개씩 진행
/// </summary>
public class QuestManager : IStartable, System.IDisposable
{
    private List<QuestData> _questDataList;
    private QuestInstance _currentQuest;
    private int _currentIndex = -1;

    public QuestInstance CurrentQuest => _currentQuest;
    public int CurrentIndex => _currentIndex;

    // 퀘스트 전환 시 (새 퀘스트 시작, 모든 퀘스트 완료)
    public event System.Action<QuestInstance> OnQuestChanged;

    // Claimable 상태 전환 시
    public event System.Action<QuestInstance> OnQuestClaimable;

    // 진행도 변경 시
    public event System.Action<QuestInstance> OnProgressChanged;

    [Inject]
    public QuestManager(List<QuestData> questDataList)
    {
        _questDataList = questDataList;
    }

    public void Start()
    {
        StartNextQuest();
    }

    public void StartNextQuest()
    {
        _currentIndex++;

        if (_currentIndex >= _questDataList.Count)
        {
            Debug.Log("QuestManager: 모든 퀘스트 완료");
            UnsubscribeCurrentQuest();
            _currentQuest = null;
            OnQuestChanged?.Invoke(null);
            return;
        }

        UnsubscribeCurrentQuest();

        var data = _questDataList[_currentIndex];

        int baseValue = 0;
        if (data.TargetEvent == QuestEventType.CustomerServed)
            baseValue = CustomerServedCounter.Instance != null
                ? CustomerServedCounter.Instance.TotalServedCount
                : 0;

        _currentQuest = new QuestInstance(data, baseValue);
        _currentQuest.OnStateChanged += OnCurrentQuestStateChanged;
        _currentQuest.OnProgressChanged += OnCurrentQuestProgressChanged;

        SubscribeCurrentQuest();

        // 퀘스트 전환 시에만 발행
        OnQuestChanged?.Invoke(_currentQuest);
    }

    public void ClaimCurrentQuest()
    {
        if (_currentQuest == null) return;
        if (_currentQuest.State != QuestState.Claimable) return;

        _currentQuest.Claim();
    }

    private void OnCurrentQuestStateChanged()
    {
        if (_currentQuest == null) return;

        if (_currentQuest.State == QuestState.Claimable)
        {
            OnQuestClaimable?.Invoke(_currentQuest);
        }
        else if (_currentQuest.State == QuestState.Completed)
        {
            StartNextQuest();
        }
    }

    private void OnCurrentQuestProgressChanged()
    {
        OnProgressChanged?.Invoke(_currentQuest);
    }

    private void SubscribeCurrentQuest()
    {
        if (_currentQuest == null) return;

        switch (_currentQuest.Data.TargetEvent)
        {
            case QuestEventType.CarrotHarvested:
                EventBus.Subscribe<CarrotHarvestedEvent>(_currentQuest.OnCarrotHarvested);
                break;
            case QuestEventType.SoupProduced:
                EventBus.Subscribe<SoupProducedEvent>(_currentQuest.OnSoupProduced);
                break;
            case QuestEventType.SoupSold:
                EventBus.Subscribe<SoupSoldEvent>(_currentQuest.OnSoupSold);
                break;
            case QuestEventType.MilkSold:
                EventBus.Subscribe<MilkSoldEvent>(_currentQuest.OnMilkSold);
                break;
            case QuestEventType.CustomerServed:
                EventBus.Subscribe<CustomerServedEvent>(_currentQuest.OnCustomerServed);
                break;
            case QuestEventType.ToolUpgraded:
                EventBus.Subscribe<ToolUpgradedEvent>(_currentQuest.OnToolUpgraded);
                break;
            case QuestEventType.FarmerHired:
                EventBus.Subscribe<FarmerHiredEvent>(_currentQuest.OnFarmerHired);
                break;
            case QuestEventType.CourierHired:
                EventBus.Subscribe<CourierHiredEvent>(_currentQuest.OnCourierHired);
                break;
            case QuestEventType.PenExpanded:
                EventBus.Subscribe<PenExpandedEvent>(_currentQuest.OnPenExpanded);
                break;
        }
    }

    private void UnsubscribeCurrentQuest()
    {
        if (_currentQuest == null) return;

        _currentQuest.OnStateChanged -= OnCurrentQuestStateChanged;
        _currentQuest.OnProgressChanged -= OnCurrentQuestProgressChanged;

        switch (_currentQuest.Data.TargetEvent)
        {
            case QuestEventType.CarrotHarvested:
                EventBus.Unsubscribe<CarrotHarvestedEvent>(_currentQuest.OnCarrotHarvested);
                break;
            case QuestEventType.SoupProduced:
                EventBus.Unsubscribe<SoupProducedEvent>(_currentQuest.OnSoupProduced);
                break;
            case QuestEventType.SoupSold:
                EventBus.Unsubscribe<SoupSoldEvent>(_currentQuest.OnSoupSold);
                break;
            case QuestEventType.MilkSold:
                EventBus.Unsubscribe<MilkSoldEvent>(_currentQuest.OnMilkSold);
                break;
            case QuestEventType.CustomerServed:
                EventBus.Unsubscribe<CustomerServedEvent>(_currentQuest.OnCustomerServed);
                break;
            case QuestEventType.ToolUpgraded:
                EventBus.Unsubscribe<ToolUpgradedEvent>(_currentQuest.OnToolUpgraded);
                break;
            case QuestEventType.FarmerHired:
                EventBus.Unsubscribe<FarmerHiredEvent>(_currentQuest.OnFarmerHired);
                break;
            case QuestEventType.CourierHired:
                EventBus.Unsubscribe<CourierHiredEvent>(_currentQuest.OnCourierHired);
                break;
            case QuestEventType.PenExpanded:
                EventBus.Unsubscribe<PenExpandedEvent>(_currentQuest.OnPenExpanded);
                break;
        }
    }

    public void Dispose()
    {
        UnsubscribeCurrentQuest();
    }
}

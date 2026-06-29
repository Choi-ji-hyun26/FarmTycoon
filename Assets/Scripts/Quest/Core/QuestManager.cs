using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 퀘스트 진행 전체 관리
/// VContainer로 등록, 싱글톤 x
/// 퀘스트 순서대로 1개씩 진행
/// </summary>
public class QuestManager : IStartable, System.IDisposable
{
    [SerializeField] private List<QuestData> _questDataList;

    private QuestInstance _currentQuest;
    private int _currentIndex = -1;

    public QuestInstance CurrentQuest => _currentQuest;
    public int CurrentIndex => _currentIndex;

    // UI 갱신용 이벤트
    public event System.Action<QuestInstance> OnQuestChanged;

    [Inject]
    public QuestManager(List<QuestData> questDataList)
    {
        _questDataList = questDataList;
    }

    // IStartable — LifetimeScope 초기화 시 자동 호출
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
            _currentQuest = null;
            OnQuestChanged?.Invoke(null);
            return;
        }

        UnsubscribeCurrentQuest();

        var data = _questDataList[_currentIndex];

        // CustomerServed 퀘스트는 현재 누적 손님 수를 기준값으로 전달
        int baseValue = 0;
        if (data.TargetEvent == QuestEventType.CustomerServed)
            baseValue = CustomerServedCounter.Instance != null
                ? CustomerServedCounter.Instance.TotalServedCount
                : 0;

        _currentQuest = new QuestInstance(data, baseValue);
        _currentQuest.OnStateChanged += OnCurrentQuestStateChanged;

        SubscribeCurrentQuest();

        OnQuestChanged?.Invoke(_currentQuest);
    }

    // 플레이어가 패널 클릭 시 UI에서 호출
    public void ClaimCurrentQuest()
    {
        if (_currentQuest == null) return;
        if (_currentQuest.State != QuestState.Claimable) return;

        _currentQuest.Claim();
    }

    private void OnCurrentQuestStateChanged()
    {
        if (_currentQuest == null) return;

        // Claimable 상태 변경은 UI가 OnQuestChanged로 감지
        OnQuestChanged?.Invoke(_currentQuest);

        // Completed → 즉시 다음 퀘스트
        if (_currentQuest.State == QuestState.Completed)
            StartNextQuest();
    }

    private void SubscribeCurrentQuest()
    {
        if (_currentQuest == null) return;

        switch (_currentQuest.Data.TargetEvent)
        {
            case QuestEventType.CarrotHarvested:
                EventBus<CarrotHarvestedEvent>.Subscribe(_currentQuest.OnCarrotHarvested);
                break;
            case QuestEventType.SoupProduced:
                EventBus<SoupProducedEvent>.Subscribe(_currentQuest.OnSoupProduced);
                break;
            case QuestEventType.SoupSold:
                EventBus<SoupSoldEvent>.Subscribe(_currentQuest.OnSoupSold);
                break;
            case QuestEventType.MilkSold:
                EventBus<MilkSoldEvent>.Subscribe(_currentQuest.OnMilkSold);
                break;
            case QuestEventType.CustomerServed:
                EventBus<CustomerServedEvent>.Subscribe(_currentQuest.OnCustomerServed);
                break;
            case QuestEventType.ToolUpgraded:
                EventBus<ToolUpgradedEvent>.Subscribe(_currentQuest.OnToolUpgraded);
                break;
            case QuestEventType.FarmerHired:
                EventBus<FarmerHiredEvent>.Subscribe(_currentQuest.OnFarmerHired);
                break;
            case QuestEventType.CourierHired:
                EventBus<CourierHiredEvent>.Subscribe(_currentQuest.OnCourierHired);
                break;
            case QuestEventType.PenExpanded:
                EventBus<PenExpandedEvent>.Subscribe(_currentQuest.OnPenExpanded);
                break;
        }
    }

    private void UnsubscribeCurrentQuest()
    {
        if (_currentQuest == null) return;

        switch (_currentQuest.Data.TargetEvent)
        {
            case QuestEventType.CarrotHarvested:
                EventBus<CarrotHarvestedEvent>.Unsubscribe(_currentQuest.OnCarrotHarvested);
                break;
            case QuestEventType.SoupProduced:
                EventBus<SoupProducedEvent>.Unsubscribe(_currentQuest.OnSoupProduced);
                break;
            case QuestEventType.SoupSold:
                EventBus<SoupSoldEvent>.Unsubscribe(_currentQuest.OnSoupSold);
                break;
            case QuestEventType.MilkSold:
                EventBus<MilkSoldEvent>.Unsubscribe(_currentQuest.OnMilkSold);
                break;
            case QuestEventType.CustomerServed:
                EventBus<CustomerServedEvent>.Unsubscribe(_currentQuest.OnCustomerServed);
                break;
            case QuestEventType.ToolUpgraded:
                EventBus<ToolUpgradedEvent>.Unsubscribe(_currentQuest.OnToolUpgraded);
                break;
            case QuestEventType.FarmerHired:
                EventBus<FarmerHiredEvent>.Unsubscribe(_currentQuest.OnFarmerHired);
                break;
            case QuestEventType.CourierHired:
                EventBus<CourierHiredEvent>.Unsubscribe(_currentQuest.OnCourierHired);
                break;
            case QuestEventType.PenExpanded:
                EventBus<PenExpandedEvent>.Unsubscribe(_currentQuest.OnPenExpanded);
                break;
        }
    }

    // IDisposable — LifetimeScope 파괴 시 자동 호출
    public void Dispose()
    {
        UnsubscribeCurrentQuest();
        if (_currentQuest != null)
            _currentQuest.OnStateChanged -= OnCurrentQuestStateChanged;
    }
}

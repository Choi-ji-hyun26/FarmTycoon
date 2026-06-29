using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// 퀘스트 진행 전체 관리
/// VContainer로 등록, 싱글톤 아님
/// 퀘스트 순서대로 1개씩 진행
/// 이벤트 구독은 QuestProgressTracker에 위임
/// </summary>
public class QuestManager : IStartable, System.IDisposable
{
    private readonly List<QuestData> _questDataList;
    private readonly QuestProgressTracker _progressTracker;

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
    public QuestManager(List<QuestData> questDataList, QuestProgressTracker progressTracker)
    {
        _questDataList = questDataList;
        _progressTracker = progressTracker;
    }

    // QuestProgressTracker.Start()가 먼저 실행되도록
    // VContainer 등록 순서로 보장 (Configure에서 Tracker 먼저 등록)
    public void Start()
    {
        StartNextQuest();
    }

    public void StartNextQuest()
    {
        _currentIndex++;

        _progressTracker.UnregisterActiveQuest();

        if (_currentIndex >= _questDataList.Count)
        {
            Debug.Log("QuestManager: 모든 퀘스트 완료");
            _currentQuest = null;
            OnQuestChanged?.Invoke(null);
            return;
        }

        var data = _questDataList[_currentIndex];

        _currentQuest = new QuestInstance(data);
        _currentQuest.OnStateChanged += OnCurrentQuestStateChanged;
        _currentQuest.OnProgressChanged += OnCurrentQuestProgressChanged;

        // 퀘스트 전환 알림 먼저 (UI 초기화)
        OnQuestChanged?.Invoke(_currentQuest);

        // 트래커에 등록 — 시작 시점 진행도 즉시 계산/통보
        // 이미 충족된 ACTION 퀘스트는 여기서 바로 Claimable 됨
        _progressTracker.RegisterActiveQuest(_currentQuest);
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
            _currentQuest.OnStateChanged -= OnCurrentQuestStateChanged;
            _currentQuest.OnProgressChanged -= OnCurrentQuestProgressChanged;
            StartNextQuest();
        }
    }

    private void OnCurrentQuestProgressChanged()
    {
        OnProgressChanged?.Invoke(_currentQuest);
    }

    public void Dispose()
    {
        if (_currentQuest != null)
        {
            _currentQuest.OnStateChanged -= OnCurrentQuestStateChanged;
            _currentQuest.OnProgressChanged -= OnCurrentQuestProgressChanged;
        }
    }
}

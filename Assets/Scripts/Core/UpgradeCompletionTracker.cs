using UnityEngine;
using UnityEngine.Events;

/*
역할
1. 게임 내 모든 업그레이드 존 완료 상태를 중앙에서 추적
2. 완료 수가 requiredCount에 도달하면 OnAllUpgradesCompleted 발행
추적 대상 (requiredCount = 5)
  - ToolUpgradeController x2 (도구 업그레이드 1, 2)
  - FarmerHireController x1 (농부 고용)
  - CourierHireController x1 (배달원 고용)
  - PenExpansionController x1 (우리 1차 확장)
*/
public class UpgradeCompletionTracker : MonoBehaviour
{
    public static UpgradeCompletionTracker Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int requiredCount = 5;

    [Header("Events")]
    [SerializeField] private UnityEvent onAllUpgradesCompleted;

    private int completedCount;

    private void Awake()
    {
        Instance = this;
    }

    public void NotifyUpgradeCompleted()
    {
        completedCount++;
        if (completedCount >= requiredCount)
            onAllUpgradesCompleted?.Invoke();
    }
}

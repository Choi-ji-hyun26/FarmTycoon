using System;
using UnityEngine;

/// <summary>
/// 경험치 및 레벨 관리
/// VContainer로 주입되어 사용 (싱글톤 아님)
/// 고정형 곡선: 레벨당 ExpPerLevel(100) EXP 필요
///
/// 추후 레벨업 보상(이동속도 등) 연동 시 OnLevelUp 구독
/// </summary>
public class ExpManager : MonoBehaviour
{
    [SerializeField] private int expPerLevel = 100;

    private int _currentLevel = 1;
    private int _currentLevelExp; // 현재 레벨 내에서의 누적 EXP
    private int _totalExp;        // 전체 누적 EXP

    public int CurrentLevel    => _currentLevel;
    public int CurrentLevelExp => _currentLevelExp;
    public int ExpPerLevel     => expPerLevel;
    public int TotalExp        => _totalExp;

    // 현재 레벨 EXP 변경 시 (currentLevelExp, expPerLevel)
    public event Action<int, int> OnExpChanged;

    // 레벨업 시 (newLevel)
    public event Action<int> OnLevelUp;

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        _totalExp += amount;
        _currentLevelExp += amount;

        // 누적분이 여러 레벨을 넘길 수 있으므로 while
        while (_currentLevelExp >= expPerLevel)
        {
            _currentLevelExp -= expPerLevel;
            _currentLevel++;
            OnLevelUp?.Invoke(_currentLevel);
        }

        OnExpChanged?.Invoke(_currentLevelExp, expPerLevel);
    }
}

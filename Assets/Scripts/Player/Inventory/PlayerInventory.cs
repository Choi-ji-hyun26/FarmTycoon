using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] private int maxCarrotCount = 10;
    [SerializeField] private int maxSoupCount = 10;
    [SerializeField] private int maxMilkCount = 10;

    [Header("References")]
    [SerializeField] private PlayerStackController stackController;

    public int CarrotCount { get; private set; }
    public int SoupCount { get; private set; }
    public int MilkCount { get; private set; }
    public int MoneyCount { get; private set; }

    public int MaxCarrotCount => maxCarrotCount;
    public bool IsCarrotFull => CarrotCount >= maxCarrotCount;
    public bool IsSoupFull => SoupCount >= maxSoupCount;
    public bool IsMilkFull => MilkCount >= maxMilkCount;

    // 도구 업그레이드 시 인벤토리 최대 용량 확장
    // 현재 값보다 클 때만 반영
    public void ExpandCapacity(int newMax)
    {
        if (newMax <= maxCarrotCount) return;
        maxCarrotCount = newMax;
        maxSoupCount   = newMax;
        maxMilkCount   = newMax;
        stackController?.ExpandStackCapacity(newMax);
    }

    // 당근 추가
    public bool TryAddCarrot(int amount)
    {
        if (amount <= 0)
            return false;
        if (CarrotCount + amount > maxCarrotCount)
            return false;

        CarrotCount += amount;
        stackController?.AddCarrotStack(amount);
        EventBus<CarrotHarvestedEvent>.Publish(new CarrotHarvestedEvent { count = amount });
        return true;
    }

    // 당근 소비
    public bool TryTakeCarrot(int amount)
    {
        if (amount <= 0)
            return false;
        if (CarrotCount < amount)
            return false;

        CarrotCount -= amount;
        stackController?.RemoveCarrotStack(amount);
        return true;
    }

    // 수프 추가
    public bool TryAddSoup(int amount)
    {
        if (amount <= 0)
            return false;
        if (SoupCount + amount > maxSoupCount)
            return false;

        SoupCount += amount;
        stackController?.AddSoupStack(amount);
        return true;
    }

    // 수프 소비
    public bool TryTakeSoup(int amount)
    {
        if (amount <= 0)
            return false;
        if (SoupCount < amount)
            return false;

        SoupCount -= amount;
        stackController?.RemoveSoupStack(amount);
        return true;
    }

    // 우유 추가
    public bool TryAddMilk(int amount)
    {
        if (amount <= 0) return false;
        if (MilkCount + amount > maxMilkCount) return false;

        MilkCount += amount;
        stackController?.AddMilkStack(amount);
        return true;
    }

    // 우유 소비
    public bool TryTakeMilk(int amount)
    {
        if (amount <= 0) return false;
        if (MilkCount < amount) return false;

        MilkCount -= amount;
        stackController?.RemoveMilkStack(amount);
        return true;
    }

    // 돈 추가 — 비주얼은 포물선 이펙트로 대체
    public bool TryAddMoney(int amount)
    {
        if (amount <= 0)
            return false;

        MoneyCount += amount;
        return true;
    }

    // 돈 소비
    public bool TryTakeMoney(int amount)
    {
        if (amount <= 0)
            return false;
        if (MoneyCount < amount)
            return false;

        MoneyCount -= amount;
        return true;
    }
}

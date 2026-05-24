
using UnityEngine;
/*
역할
1. 수프(오른쪽), 우유(왼쪽) 스택 수량 및 비주얼 관리
2. 손님에게 아이템 공급 (수프 우선)
3. 보상 돈 비주얼 관리
*/
public class SaleDeskController : MonoBehaviour
{
    [Header("Soup Stack (오른쪽)")]
    [SerializeField] private ItemStack soupStack;

    [Header("Milk Stack (왼쪽)")]
    [SerializeField] private ItemStack milkStack;

    [Header("Delivery Point")]
    [SerializeField] private Transform inputPoint;
    public Transform InputPoint => inputPoint; // for 배달 인력 이동 목표
    
    [Header("Timing")]
    [SerializeField] private float supplyInterval = 0.2f;
    private float supplyTimer = 0f;

    [Header("Target Prisoner")]
    [SerializeField] private CustomerController targetCustomer;

    [Header("Money Visual")]
    [SerializeField] private ItemStack moneyStack;
    [SerializeField] private int moneyValuePerVisual = 10;

    private int storedMoneyAmount;

    public int StoredSoupCount  => soupStack.StoredCount;
    public int StoredMilkCount  => milkStack.StoredCount;
    public int StoredMoneyAmount => storedMoneyAmount;

    // 수프 납품용
    public bool CanAddSoup(int amount = 1) => soupStack.CanAdd(amount);
    public bool TryAddSoup(int amount = 1) => soupStack.TryAdd(amount);

    // 우유 납품용
    public bool CanAddMilk(int amount = 1) => milkStack.CanAdd(amount);
    public bool TryAddMilk(int amount = 1) => milkStack.TryAdd(amount);

    private void Update()
    {
        supplyTimer += Time.deltaTime;
        TrySupplyCustomer();
    }

    // 현재 공급 대상 수감자 설정
    public void SetTargetCustomer(CustomerController customer)
    {
        targetCustomer = customer;
    }

    // 손님에게 아이템 제공, 수프 우선, 없으면 우유
    // 수감자에게 수갑 1개씩 공급, 감옥 만석 or 대상 상태 맞지 않으면 공급 중단    
    private void TrySupplyCustomer()
    {
        if(targetCustomer == null) return;
        if(targetCustomer.IsCompleted) return;
        if(targetCustomer.IsMovingToWaitingSlot) return; // front로 이동 중일 때는 지급 금지
        if(supplyTimer < supplyInterval) return;

        supplyTimer = 0f;

        // 손님 요구 타입에 맞는 아이템 공급
        if (targetCustomer.RequestType == CustomerRequestType.Soup)
        {
            if(!soupStack.IsEmpty)
                if(soupStack.TryConsume(1))
                    targetCustomer.TryReceiveProduct(1);
        }
        else if (targetCustomer.RequestType == CustomerRequestType.Milk)
        {
            if (!milkStack.IsEmpty)
                if (milkStack.TryConsume(1))
                    targetCustomer.TryReceiveProduct(1);
        }
    }

    // Desk에 돈을 추가하려 시도, 최대 보관량 넘기면 실패
    public void AddMoney(int moneyValue, int visualCount)
    {
        if (moneyValue <= 0 || visualCount <= 0) return;

        storedMoneyAmount += moneyValue;
        moneyStack.TryAdd(visualCount);

        Sfx.Play(SoundId.MoneyCreate);
    }

    public bool HasMoney(int amount = 1) => storedMoneyAmount >= amount;

    // 플레이어가 돈 픽업 시 비주얼 제거
    public bool TryTakeMoney(int moneyValuePerStack = 10)
    {

        if(storedMoneyAmount < moneyValuePerStack) return false;
        if(moneyStack.IsEmpty) return false;

        storedMoneyAmount -= moneyValuePerStack;
        moneyStack.TryConsume(1);
        
        return true;
    }
}

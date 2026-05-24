using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 당근 입력 수량 및 비주얼 관리
2. 일정 주기마다 당근 → 수프 변환 처리
3. 수프 출력 수량 및 비주얼 관리
4. 농부 NPC 수확 당근 포물선 연출 (VisualEffectPool 사용)
*/
public class CookingMachineController : MonoBehaviour, IPickupSource, IItemDepositTarget
{
    [Header("Input")]
    [SerializeField] private int storedCarrotCount = 0;
    [SerializeField] private int maxStoredCarrotCount = 30;

    [Header("Output")]
    [SerializeField] private int producedSoupCount = 0;
    [SerializeField] private int maxProducedSoupCount = 30;

    [Header("Carrot Visual Stack")]
    [SerializeField] private ItemStack carrotStack;
    
    [Header("Soup Visual Stack")]
    [SerializeField] private ItemStack soupStack;

    [Header("Carrot Move Animation")]
    [SerializeField] private ItemVisualEffectPool carrotEffectPool;
    [SerializeField] private Transform stackedCarrotPoint;
    [SerializeField] private float moveToInputDuration = 1f; // 이동 시간
    [SerializeField] private float moveHeight = 3f;        // 포물선 높이
    [SerializeField] private float carrotStackYOffset = 0.2f;

    [Header("Process")]
    [SerializeField] private float processInterval = 1.0f;
    private float processTimer = 0f;

    // 농부가 수확한 당근 이동 관련 변수
    private int reservedCount = 0; // 이동 중인 예약 수량

    public bool IsFull =>(storedCarrotCount + reservedCount) >= maxStoredCarrotCount;

    public int StoredCarrotCount => storedCarrotCount;
    public int ProducedsoupCount => producedSoupCount;
    public int MaxStoredCarrotCount => maxStoredCarrotCount;
    public int MaxProducedSoupCount => maxProducedSoupCount;

    // IItemDepositTarget — 당근 입력
    public bool CanAddItem(int amount = 1) => CanAddCarrot(amount);
    public bool TryAddItem(int amount = 1) => TryAddCarrot(amount);

    // IPickupSource  — 수프 출력
    public int AvailableCount => producedSoupCount;
    public bool HasItem(int amount = 1) => HasSoup(amount);
    public bool TryTakeItem(int amount = 1) => TryTakeSoup(amount);

    private void Update()
    {
        ProcessMachine();
    }

    // 플레이어가 당근을 직접 납품할 때 호출
    public bool TryAddCarrot(int amount = 1)
    {
        if(amount <= 0) return false;
        if(!CanAddCarrot(amount)) return false;

        storedCarrotCount += amount;
        carrotStack.TryAdd(amount);

        return true;
    }

    public bool CanAddCarrot(int amount = 1) =>
        storedCarrotCount + amount <= maxStoredCarrotCount;

    public bool HasSoup(int amount = 1) =>
        producedSoupCount >= amount;

    // 수프를 출력 슬롯에서 제거, 비주얼 제거
    public bool TryTakeSoup(int amount = 1)
    {
        if (!HasSoup(amount)) return false;

        producedSoupCount -= amount;
        soupStack.TryConsume(amount);

        return true;
    }

    private bool CanProduceSoup(int amount = 1) =>
        producedSoupCount + amount <= maxProducedSoupCount;

    // 수프를 출력 슬롯에서 제거, 비주얼 제거
    private void ProcessMachine()
    {
        if(storedCarrotCount <= 0) return;
        if(!CanProduceSoup()) return;
    
        processTimer += Time.deltaTime;

        if(processTimer >= processInterval)
        {
            processTimer = 0f;
            ConsumeCarrotAndProduceSoup();
        }
    }

    // 당근 1개 소비 → 수프 1개 생산
    private void ConsumeCarrotAndProduceSoup()
    {
        if(storedCarrotCount <= 0) return;
        if(!CanProduceSoup()) return;
        
        storedCarrotCount--;
        producedSoupCount++;

        carrotStack.TryConsume(1);
        soupStack.TryAdd(1);
        
        Sfx.PlayAtPoint(SoundId.ProductCreate, transform.position);

    }
    // 플레이어 관련 코드 아님
    // 농부가 당근 수확 시 호출
    // 당근 프리팹이 농부 위치에서 input zone으로 날아오는 연출 후 스택에 추가
    public bool TryAddProductWithAnimation(int amount, Vector3 fromWorldPosition)
    {
        if(amount <= 0) return false;
        if(IsFull) return false;

        for(int i = 0; i < amount; i++)
        {
            int slotIndex = storedCarrotCount + reservedCount;
            reservedCount++;

            Vector3 targetWorldPos = stackedCarrotPoint.TransformPoint(
                new Vector3(0f, slotIndex * carrotStackYOffset, 0f));

            carrotEffectPool.PlayMoveAnimation(
                fromWorldPosition + Vector3.up * 0.5f,
                targetWorldPos,
                moveToInputDuration,
                moveHeight,
                onArrived: () =>
                {
                    storedCarrotCount++;
                    reservedCount--;
                    carrotStack.TryAdd(1);
                });
        }
        return true;
    }
}
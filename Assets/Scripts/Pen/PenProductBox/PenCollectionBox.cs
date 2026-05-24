using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 동물이 생산한 생산물 수량 관리
2. 플레이어가 픽업 시 수량 차감
3. 소 위치에서 박스로 날아오는 연출 (VisualEffectPool 사용)
*/
public class PenCollectionBox : MonoBehaviour, IPickupSource
{
    [Header("Capacity")]
    [SerializeField] private int maxStoredProductCount = 10;

    [Header("Product Visual Stack")]
    [SerializeField] private ItemStack milkStack;

    [Header("Milk Move Animation")]
    [SerializeField] private ItemVisualEffectPool milkEffectPool;
    [SerializeField] private Transform milkStackPoint;
    [SerializeField] private float moveToBoxDuration = 1f; // 이동 시간
    [SerializeField] private float moveHeight = 1.5f;      // 포물선 높이
    [SerializeField] private float milkStackYOffset = 0.2f;

    private int storedProductCount = 0;
    private int reservedCount = 0; // 이동 중인 예약 수량

    public bool IsFull =>(storedProductCount + reservedCount) >= maxStoredProductCount;
    public bool IsEmpty => storedProductCount <= 0;
    public int CurrentAmount => storedProductCount;

    // Pen 확장 시 우유 최대 보관 용량 확장
    public void ExpandCapacity(int newMax)
    {
        if (newMax <= maxStoredProductCount) return;
        maxStoredProductCount = newMax;
    }

    // IPickupSource
    public int AvailableCount => storedProductCount;
    public bool HasItem(int amount = 1) => !IsEmpty && CurrentAmount >= amount;
    public bool TryTakeItem(int amount = 1) => TryPickup(amount);

    // 동물이 생산물 추가 시도
    // 소 위치에서 박스로 날아오는 연출 후 스택에 추가
    public bool TryAddProductWithAnimation(int amount, Vector3 fromWorldPosition)
    {
        if (amount <= 0) return false;
        if (IsFull) return false;

        for (int i = 0; i < amount; i++){
            //int slotIndex = milkStackVisuals.Count + reservedCount;
            int slotIndex = storedProductCount + reservedCount;
            reservedCount++;

            Vector3 targetWorldPos = milkStackPoint.TransformPoint(
                new Vector3(0f, slotIndex * milkStackYOffset, 0f));
            
            milkEffectPool.PlayMoveAnimation(
                fromWorldPosition + Vector3.up * 0.5f,
                targetWorldPos,
                moveToBoxDuration,
                moveHeight,
                onArrived: () =>
                {
                    storedProductCount++;
                    reservedCount--;
                    milkStack.TryAdd(1);
                });
        }

        return true;
    }

    // 플레이어가 수거함에서 amount 만큼 픽업 시도
    // 실제 픽업된 수량 반환
    public bool TryPickup(int amount)
    {
        if (IsEmpty) return false;
        if (storedProductCount < amount) return false;

        storedProductCount -= amount;
        milkStack.TryConsume(amount);
        return true;
    }
}

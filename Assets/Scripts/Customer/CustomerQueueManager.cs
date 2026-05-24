using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 랜덤 요구량 생성
2. 큐 관리
3. 완료된 손님 보상 처리
4. 데스크에 돈 비주얼 생성 요청
*/
public class CustomerDeskQueueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomerController customerPrefab;
    [SerializeField] private SaleDeskController saleDeskController;

    [Header("Queue Slots")]
    [SerializeField] private Transform[] waitingSlots; // frontSlot, backSlot 대신 배열로 관리
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn")]
    [SerializeField] private int queueSize = 2;
    [SerializeField] private Vector2Int soupRequirementRange = new Vector2Int(1, 5);
    
    [Header("Milk Unlock")]
    [SerializeField] private Vector2Int milkRequirementRange = new Vector2Int(1, 2);
    private bool isMilkUnlocked = false;

    private readonly Queue<CustomerController> waitingQueue = new();

    [Header("Spawn Interval")]
    [SerializeField] private float spawnInterval = 0.5f; // 손님 간 스폰 텀

    private void Start()
    {
        StartCoroutine(FillQueueRoutine());
    }

    // 손님을 spawnInterval 간격으로 순서대로 스폰
    private IEnumerator FillQueueRoutine()
    {
        while (waitingQueue.Count < queueSize)
        {
            SpawnAndEnqueueCustomer();
            UpdateQueueSlots();
            UpdateDeskTarget();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void FillQueue()
    {
        StartCoroutine(FillQueueRoutine());
    }
    // 첫 번째 펜 업그레이드 완료 시 외부에서 호출
    public void UnlockMilkCustomer()
    {
        isMilkUnlocked = true;
    }

    private void SpawnAndEnqueueCustomer()
    {
        if (spawnPoint == null)
            return;

        CustomerController customer = Instantiate(
            customerPrefab, 
            spawnPoint.position, 
            Quaternion.identity);

        CustomerRequestType type = DecideRequestType();
        int amount = DecideRequestAmount(type);

        customer.Initialize(amount, type);
        customer.OnCompleted += HandleCustomerCompleted;
        waitingQueue.Enqueue(customer);
    }

    // Pen 해금 전 -> 수프만, 해금 후 -> 수프/우유 랜덤
    private CustomerRequestType DecideRequestType()
    {
        if(!isMilkUnlocked)
            return CustomerRequestType.Soup;

        return Random.value < 0.5f ?
            CustomerRequestType.Soup
            : CustomerRequestType.Milk;
    }

    private int DecideRequestAmount(CustomerRequestType type)
    {
        return type == CustomerRequestType.Soup
            ? Random.Range(soupRequirementRange.x, soupRequirementRange.y + 1)
            : Random.Range(milkRequirementRange.x, milkRequirementRange.y + 1);
    }

    private void HandleCustomerCompleted(CustomerController completedCustomer)
    {

        if (waitingQueue.Count == 0 || completedCustomer == null){
            return;
        }

        CustomerController front = waitingQueue.Peek();

        if (front != completedCustomer)
            return;

        waitingQueue.Dequeue();
        completedCustomer.OnCompleted -= HandleCustomerCompleted;

        int rewardMoneyValue = completedCustomer.RewardAmount;              // 실제 돈 30
        int rewardMoneyVisualCount = completedCustomer.RequiredProducts;   // 비주얼 3개

        if (saleDeskController != null)
        {
            saleDeskController.AddMoney(rewardMoneyValue, rewardMoneyVisualCount);
        }
        else
        {
            Debug.LogWarning("SaleDeskController가 null");
        }

        FillQueue();
        UpdateQueueSlots();
        UpdateDeskTarget();
    }

    private void UpdateQueueSlots()
    {
        CustomerController[] customers = waitingQueue.ToArray();

        // 슬롯 배열 기준으로 순서대로 배치 — 슬롯 수 늘려도 코드 변경 불필요
        for (int i = 0; i < Mathf.Min(customers.Length, waitingSlots.Length); i++)
        {
            if (customers[i] != null)
                customers[i].SetWaitingSlot(waitingSlots[i]);
        }
    }

    private void UpdateDeskTarget()
    {
        if (waitingQueue.Count == 0)
        {
            saleDeskController.SetTargetCustomer(null);
            return;
        }

        CustomerController[] customers = waitingQueue.ToArray();

        for (int i = 0; i < customers.Length; i++)
        {
            bool isTarget = (i == 0);
            customers[i].SetRequirementUIVisible(isTarget);
        }

        saleDeskController.SetTargetCustomer(customers[0]);
    }
}
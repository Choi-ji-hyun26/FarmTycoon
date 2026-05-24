using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 아이템 스택 수량 관리
2. 스택 비주얼 생성/제거 (Object Pooling 적용)
3. PlayerStackController, SaleDeskController 등에서 공통 사용
*/
[System.Serializable]
public class ItemStack
{
    [SerializeField] private Transform stackPoint;
    [SerializeField] private GameObject stackPrefab;
    [SerializeField] private float stackYOffset = 0.2f;
    [SerializeField] private int maxCount = 10;

    private readonly List<GameObject> visuals = new();
    private readonly Queue<GameObject> pool = new();

    private int storedCount;

    public int StoredCount => storedCount;
    public int MaxCount => maxCount;
    public bool IsFull => storedCount >= maxCount;
    public bool IsEmpty => storedCount <= 0;
    public bool CanAdd(int amount = 1) => storedCount + amount <= maxCount;

    // 용량 확장 — ExpandCapacity 호출 시 ItemStack maxCount 동기화
    public void SetMaxCount(int newMax)
    {
        if (newMax <= maxCount) return;
        maxCount = newMax;
    }

    // 아이템 추가 및 비주얼 생성 (pool에서 꺼냄)
    public bool TryAdd(int amount = 1)
    {
        if(amount <= 0) return false;
        if(!CanAdd(amount)) return false;

        storedCount += amount;
        for(int i = 0; i < amount; i++)
        {
            SpawnVisual();
        }
        return true;
    }

    // 아이템 소비 및 비주얼 제거 (pool에서 반환)
    public bool TryConsume(int amount = 1)
    {
        if(amount <= 0) return false;
        if(storedCount < amount) return false;

        storedCount -= amount;
        for(int i = 0; i < amount; i++)
        {
            RemoveTopVisual();
        }
        return true;
    }
    // pool에서 오브젝트 꺼내기, 비어있으면 새로 생성
    private GameObject GetFromPool()
    {
        if(pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return Object.Instantiate(stackPrefab, stackPoint);
    }

    // 오브젝트를 Pool에 반환, 비활성화 후 stackPoint 하위로 이동
    private void ReturnToPool(GameObject obj)
    {
        if(obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(stackPoint);
        pool.Enqueue(obj);
    }

    // Pool에서 꺼내 스택 위치에 배치
    private void SpawnVisual()
    {
        if(stackPrefab == null || stackPoint == null){
            Debug.LogWarning("ItemStack: stackPrefab 또는 stackPoint가 null");
            return;
        }

        GameObject obj = GetFromPool(); 
        obj.transform.SetParent(stackPoint);
        obj.transform.localPosition = new Vector3(0f, visuals.Count * stackYOffset, 0f);
        obj.transform.localRotation = Quaternion.identity;
        visuals.Add(obj);
    }

    private void RemoveTopVisual()
    {
        if(visuals.Count <= 0) return;

        int last = visuals.Count - 1;
        GameObject top = visuals[last];
        visuals.RemoveAt(last);

        ReturnToPool(top);
    }
}

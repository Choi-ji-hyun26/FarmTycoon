using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 플레이어가 들고 있는 아이템 시각 스택 관리
2. front: 수프, 우유 (ItemStack)
3. back: 당근 (Object Pooling 적용)
*/
public class PlayerStackController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private Transform frontStackRoot;
    [SerializeField] private Transform backStackSlot;

    [Header("Front Stacks (ItemStack)")]
    [SerializeField] private ItemStack soupStack;
    [SerializeField] private ItemStack milkStack;

    [Header("Back Stack Prefabs")]
    [SerializeField] private GameObject carrotStackPrefab;

    [Header("Back Stack Settings")]
    [SerializeField] private float carrotStackYOffset = 0.25f;
    [SerializeField] private int maxVisualStack = 10;       // 당근 비주얼 상한 (도구 업그레이드로 확장)
    [SerializeField] private int initialPoolSize = 10;

    private readonly List<GameObject> carrotStacks = new();
    private readonly Queue<GameObject> carrotPool = new();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
            ReturnToPool(carrotPool, CreatePoolObject(carrotStackPrefab));
    }

    // ── Front (ItemStack) ──
    public void AddSoupStack(int amount)    => soupStack.TryAdd(amount);
    public void RemoveSoupStack(int amount) => soupStack.TryConsume(amount);

    public void AddMilkStack(int amount)    => milkStack.TryAdd(amount);
    public void RemoveMilkStack(int amount) => milkStack.TryConsume(amount);

    // 인벤토리 확장 시 soupStack / milkStack / maxVisualStack 동기화 (돈은 별도 maxMoneyVisualStack 사용)
    public void ExpandStackCapacity(int newMax)
    {
        if (newMax <= maxVisualStack) return;
        maxVisualStack = newMax;
        soupStack.SetMaxCount(newMax);
        milkStack.SetMaxCount(newMax);
    }

    // ── Back (Object Pooling) ──
    public void AddCarrotStack(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (carrotStacks.Count >= maxVisualStack) return;

            GameObject stackObj = GetFromPool(carrotPool, carrotStackPrefab);
            stackObj.transform.localRotation = Quaternion.identity;
            carrotStacks.Add(stackObj);
        }

        RebuildStackVisuals(carrotStacks, backStackSlot, carrotStackYOffset);
    }

    public void RemoveCarrotStack(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (carrotStacks.Count <= 0) return;

            int lastIndex = carrotStacks.Count - 1;
            ReturnToPool(carrotPool, carrotStacks[lastIndex]);
            carrotStacks.RemoveAt(lastIndex);
        }

        RebuildStackVisuals(carrotStacks, backStackSlot, carrotStackYOffset);
    }


    //전달받은 스택 리스트의 모든 오브젝트를 targetRoot 아래로 재배치
    private void RebuildStackVisuals(List<GameObject> stackList, Transform targetRoot, float yOffset)
    {
        if (targetRoot == null)
            return;

        for (int i = 0; i < stackList.Count; i++)
        {
            if (stackList[i] == null)
                continue;

            stackList[i].transform.SetParent(targetRoot);
            stackList[i].transform.localPosition = new Vector3(0f, i * yOffset, 0f);
            stackList[i].transform.localRotation = Quaternion.identity;
        }
    }

    // 풀에서 오브젝트 꺼내기, 비어있으면 새로 생성
    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        GameObject newObj = CreatePoolObject(prefab);
        newObj.SetActive(true); 
        return newObj;
    }

    // 오브젝트를 풀에 반환
    private void ReturnToPool(Queue<GameObject> pool, GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
    }

    // 풀용 오브젝트 새로 생성
    private GameObject CreatePoolObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }
}
using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 배달 인력 앞에 보이는 수갑 스택 비주얼 관리
2. 수량에 맞게 수갑 오브젝트 추가, 제거
*/
public class WorkerStackController : MonoBehaviour
{
    [Header("Stack Root")]
    [SerializeField] private Transform productStackRoot;

    [Header("Prefab")]
    [SerializeField] private GameObject productStackPrefab;

    [Header("Stack Settings")]
    [SerializeField] private float productStackYOffset = 0.2f;
    [SerializeField] private int maxVisualStack = 10;

    private readonly List<GameObject> productStacks = new();

    public int CurrentVisualCount => productStacks.Count;
    
    public void AddProductStack(int amount = 1)
    {
        if (productStackRoot == null || productStackPrefab == null)
            return;

        for (int i = 0; i < amount; i++)
        {
            if (productStacks.Count >= maxVisualStack)
                return;

            GameObject stackObj = Instantiate(productStackPrefab, productStackRoot);
            stackObj.transform.localPosition = new Vector3(0f, productStacks.Count * productStackYOffset, 0f);
            stackObj.transform.localRotation = Quaternion.identity;

            productStacks.Add(stackObj);
        }
    }

    public void RemoveProductStack(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (productStacks.Count <= 0)
                return;

            int lastIndex = productStacks.Count - 1;
            GameObject topObj = productStacks[lastIndex];
            productStacks.RemoveAt(lastIndex);

            if (topObj != null)
                Destroy(topObj);
        }
    }

    public void ClearproductStack()
    {
        for (int i = productStacks.Count - 1; i >= 0; i--)
        {
            if (productStacks[i] != null)
                Destroy(productStacks[i]);
        }

        productStacks.Clear();
    }
}
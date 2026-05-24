using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 포물선 이동 연출용 오브젝트 풀 관리
2. CookingMachineController, PenCollectionBox에서 공통 사용
3. 연출 완료 후 자동 풀 반환
*/

public class ItemVisualEffectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 5;

    private readonly Queue<GameObject> pool = new();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
            ReturnToPool(CreateNew());
    }

    // 풀에서 오브젝트 꺼내기
    // 풀이 비어있으면 새로 생성
    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        GameObject newObj = CreateNew();
        newObj.SetActive(true);
        return newObj;
    }

    // 오브젝트를 풀에 반환
    public void Return(GameObject obj)
    {
        if (obj == null) return;
        ReturnToPool(obj);
    }

    // 포물선 이동 연출 실행
    // 완료 후 자동으로 풀에 반환
    public void PlayMoveAnimation(
        Vector3 fromWorldPosition,
        Vector3 targetWorldPosition,
        float duration,
        float arcHeight,
        System.Action onArrived)
    {
        GameObject obj = Get();
        obj.transform.position = fromWorldPosition;
        StartCoroutine(MoveRoutine(obj, fromWorldPosition, targetWorldPosition, duration, arcHeight, onArrived));
    }

    private IEnumerator MoveRoutine(
        GameObject obj,
        Vector3 start,
        Vector3 target,
        float duration,
        float arcHeight,
        System.Action onArrived)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;
            obj.transform.position = Vector3.Lerp(start, target, t) + Vector3.up * arc;

            yield return null;
        }

        obj.transform.position = target;
        Return(obj);

        onArrived?.Invoke();
    }

    private GameObject CreateNew()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
    }
}

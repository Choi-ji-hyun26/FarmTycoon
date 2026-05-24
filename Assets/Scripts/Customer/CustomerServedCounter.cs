using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 서빙 완료된 손님 수 누적 카운트
2. 카운트 변경 시 이벤트 발생
3. PenExpansionController 등 조건 체크 구독자에게 알림
*/
public class CustomerServedCounter : MonoBehaviour
{
    public static CustomerServedCounter Instance { get; private set; }

    private int totalServedCount;
    public int TotalServedCount => totalServedCount;

    // 서빙 완료 손님 수가 변경될 때마다 발생
    // 구독자에게 누적 카운트 전달
    public event Action<int> OnServedCountChanged;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 손님 1명 서빙 완료 시 외부에서 호출
    public void NotifyServed()
    {
        totalServedCount++;
        OnServedCountChanged?.Invoke(totalServedCount);
    }
}

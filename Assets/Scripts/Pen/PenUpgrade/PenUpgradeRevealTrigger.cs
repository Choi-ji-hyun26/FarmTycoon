using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 손님 할당량 채우면 호출됨
2. 최초 1회만 우리 업그레이드 존 공개 연출 요청
연결 Object : PenUpgradeEvent
*/
public class PenUpgadeRevealTrigger : MonoBehaviour
{
   [Header("References")]
    [SerializeField] private ZoneRevealDirector zoneRevealDirector;
    [SerializeField] private GameObject penExpansionZone;
    [SerializeField] private Transform prisonFocusPoint;

    [Header("Condition")]
    [SerializeField] private int requiredCustomerCount = 5;

    private bool hasTriggered;

    private void Awake()
    {
        if(penExpansionZone != null)
            penExpansionZone.SetActive(false);
    }
    private void Start()
    {
        if (CustomerServedCounter.Instance != null)
            CustomerServedCounter.Instance.OnServedCountChanged += HandleServedCountChanged;
    }

    private void OnDisable()
    {
        if (CustomerServedCounter.Instance != null)
            CustomerServedCounter.Instance.OnServedCountChanged -= HandleServedCountChanged;
    }

    private void HandleServedCountChanged(int totalServedCount)
    {
        if(hasTriggered) return;
        if (totalServedCount < requiredCustomerCount) return;
        hasTriggered = true;
        
        if(zoneRevealDirector != null)
            zoneRevealDirector.PlayReveal(penExpansionZone, prisonFocusPoint);
        else
            penExpansionZone?.SetActive(true); // ZoneRevealDirector 없을 때 fallback
    }

}

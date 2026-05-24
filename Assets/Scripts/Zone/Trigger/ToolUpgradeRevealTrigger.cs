
using UnityEngine;

/*
역할
1. 첫 돈 획득 시 도구 업그레이드 존 공개 연출 요청
2. 1회 실행 방지
*/
public class ToolUpgradeRevealTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZoneRevealDirector zoneRevealDirector;
    [SerializeField] private GameObject toolUpgradeZone;
    [SerializeField] private Transform toolUpgradeFocusPoint;

    private bool hasTriggered;

    private void Awake()
    {
        if (toolUpgradeZone != null)
            toolUpgradeZone.SetActive(false);
    }

    // 플레이어가 처음 돈을 얻었을 때 외부에서 호출
    public void NotifyFirstMoneyAcquired()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;

        if (zoneRevealDirector != null)
        {
            zoneRevealDirector.PlayReveal(toolUpgradeZone, toolUpgradeFocusPoint);
        }
    }
}

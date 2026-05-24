using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
역할
1. 현재 채굴 도구 업그레이드 단계 관리
2. Field Zone 안/밖 상태 관리
3. 현재 채굴 스펙(반경, 동시 채굴 수) 제공
4. ToolVisualController에 현재 장착 상태 전달
*/
public class PlayerFarmingToolController : MonoBehaviour
{
    [Header("Current State")]
    [SerializeField] private FarmingToolTier unlockedTier = FarmingToolTier.Sickle;
    [SerializeField] private bool isInHarvestingZone = false;

    [Header("References")]
    [SerializeField] private PlayerZoneDetector zoneDetector;
    [SerializeField] private PlayerToolVisualController toolVisualController;

    [Header("Sickle Spec")]
    [SerializeField] private float sickleHarvestRadius = 1.2f;
    [SerializeField] private int sickleMaxTargets = 1;

    [Header("Rake Spec")]
    [SerializeField] private float rakeHarvestRadius = 1.8f;
    [SerializeField] private int rakeMaxTargets = 5;

    [Header("Vehicle Spec")]
    [SerializeField] private float vehicleHarvestRadius = 2.5f;
    [SerializeField] private int vehicleMaxTargets = 10;
 
    public FarmingToolTier CurrentTier => unlockedTier;

    public FarmingToolTier ActiveTier
    {
        get
        {
            if(!isInHarvestingZone)
                return FarmingToolTier.None;
            return unlockedTier;
        }
    }

    public float CurrentHarvestRadius
    {
        get
        {
            switch (ActiveTier)
            {
                case FarmingToolTier.Sickle:
                    return sickleHarvestRadius;
                case FarmingToolTier.Rake:
                    return rakeHarvestRadius;
                case FarmingToolTier.Vehicle:
                    return vehicleHarvestRadius;
                case FarmingToolTier.None:
                default:
                    return 0f;
            }
        }
    }

    public int CurrentMaxTargets
    {
        get
        {
            switch (ActiveTier)
            {
                case FarmingToolTier.Sickle:
                    return sickleMaxTargets;
                case FarmingToolTier.Rake:
                    return rakeMaxTargets;
                case FarmingToolTier.Vehicle:
                    return vehicleMaxTargets;
                case FarmingToolTier.None:
                default:
                    return 0;
            }
        }
    }

    private void OnEnable()
    {
        if (zoneDetector != null)
        {
            zoneDetector.OnZoneEntered += HandleZoneEntered;
            zoneDetector.OnZoneExited += HandleZoneExited;
        }
    }

    private void OnDisable()
    {
        if (zoneDetector != null)
        {
            zoneDetector.OnZoneEntered -= HandleZoneEntered;
            zoneDetector.OnZoneExited -= HandleZoneExited;
        }
    }
    private void Start()
    {
        RefreshVisual();
    }

    // 특정 도구 단계까지 해금, 현재 단계보다 높을 때만 반영
    public void UnlockTier(FarmingToolTier newTier)
    {
        if(newTier <= unlockedTier)
            return;
        unlockedTier = newTier;
        RefreshVisual();
    }
    // zone 진입 이벤트를 받아 field zone 진입 여부 판정
    private void HandleZoneEntered(Zone zone)
    {
        if(zone == null)
            return;
        
        if(zone.zoneType == ZoneType.Harvesting)
        {
            isInHarvestingZone = true;
            RefreshVisual();
        }
    }

    private void HandleZoneExited(Zone zone)
    {
        if(zone == null)
            return;

        if(zone.zoneType == ZoneType.Harvesting)
        {
            isInHarvestingZone = false;
            RefreshVisual();
        }
    }
    private void RefreshVisual()
    {
        if(toolVisualController == null)
            return;
        toolVisualController.ShowTool(ActiveTier);
    }
}

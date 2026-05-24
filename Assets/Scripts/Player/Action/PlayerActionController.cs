using UnityEngine;

/*
역할
플레이어 액션 시스템의 Coordinator
- 매 프레임 각 핸들러의 Tick()을 호출
- 핸들러 간 공유가 필요한 컨텍스트(isHarvesting, toolTier 등)를 계산해서 전달
*/
public class PlayerActionController : MonoBehaviour
{
    [Header("Handlers")]
    [SerializeField] private PlayerZoneActionHandler zoneActionHandler;
    [SerializeField] private VehicleMountController vehicleMountController;
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Player")]
    [SerializeField] private PlayerFarmer playerFarmer;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerZoneDetector zoneDetector;
    [SerializeField] private InputDispatcher dispatcher;

    private void Update()
    {
        float speed = dispatcher.GetCurrentSpeed();

        bool isHarvesting =
            zoneDetector.CurrentZone != null &&
            zoneDetector.CurrentZone.zoneType == ZoneType.Harvesting;

        FarmingToolTier toolTier = playerFarmer != null
            ? playerFarmer.CurrentToolTier
            : FarmingToolTier.Sickle;

        bool isHitting =
            isHarvesting &&
            toolTier == FarmingToolTier.Sickle &&
            playerFarmer != null &&
            playerFarmer.HasCarrotTarget;

        bool isCarrying =
            inventory.SoupCount > 0 ||
            inventory.MilkCount > 0;

        zoneActionHandler.Tick();
        vehicleMountController.Tick(isHarvesting, toolTier);
        animationController.Tick(speed, isHarvesting, isHitting, isCarrying, toolTier);
    }
}

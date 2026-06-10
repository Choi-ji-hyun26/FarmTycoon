using UnityEngine;

/*
역할
존 타입에 따라 플레이어의 픽업 / 납품 / 지불 액션을 처리
- 공통 픽업 / 납품 로직을 인터페이스 기반으로 추상화
- 존 전환 시 공통 타이머 리셋
- 첫 돈 획득 이벤트 발행
*/
public class PlayerZoneActionHandler : MonoBehaviour
{
    [Header("Zone")]
    [SerializeField] private PlayerZoneDetector zoneDetector;
    [SerializeField] private VehicleMountController vehicleMountController;

    [Header("Player")]
    [SerializeField] private PlayerFarmer playerFarmer;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerUIController maxUIController;

    [Header("Scene Objects")]
    [SerializeField] private CookingMachineController cookingMachineController;
    [SerializeField] private SaleDeskController saleDeskController;
    [SerializeField] private PenCollectionBox penCollectionBox;
    [SerializeField] private ToolUpgradeRevealTrigger toolUpgradeRevealTrigger;

    [Header("Money Pickup")]
    [SerializeField] private int moneyValuePerPickup = 10;
    [SerializeField] private int moneyValuePerDeposit = 10;
    [SerializeField] private MoneyFlyController moneyFlyController;
    [SerializeField] private Transform playerHeadPoint; // 플레이어 머리 위 Transform

    [Header("Interval")]
    [SerializeField] private float pickupInterval  = 0.2f;
    [SerializeField] private float depositInterval = 0.2f;

    private float pickupTimer     = 0f;
    private float depositTimer    = 0f;
    private float moneyPickupTimer  = 0f;
    private float moneyDepositTimer = 0f;

    private ZoneType lastZoneType;
    private bool hasNotifiedFirstMoney;

    public void Tick()
    {
        HandleZoneActions();
    }

    // Vehicle 탑승 중에는 Zone 감지와 무관하게 수확 직접 호출
    // 탑승 시 PlayerZoneDetector 위치가 SeatPoint로 강제 이동하면서
    // OnTriggerExit가 발생해 CurrentZone이 null이 되는 문제 방지
    private void HandleZoneActions()
    {
        if (vehicleMountController.IsMounted)
        {
            playerFarmer.TryExecuteHarvest();
            return;
        }

        var zone = zoneDetector.CurrentZone;
        if (zone == null) return;

        // 존이 바뀌면 공통 타이머 리셋
        if (zone.zoneType != lastZoneType)
        {
            pickupTimer       = 0f;
            depositTimer      = 0f;
            moneyDepositTimer = 0f;
            lastZoneType      = zone.zoneType;
        }

        switch (zone.zoneType)
        {
            case ZoneType.Harvesting:
                if (playerFarmer.CurrentToolTier != FarmingToolTier.Sickle)
                    playerFarmer.TryExecuteHarvest();
                break;

            case ZoneType.CookingInput:
                TryDepositCarrotToCooking();
                break;

            case ZoneType.SoupPickup:
                TryPickupSoupFromCooking();
                break;

            case ZoneType.MilkPickup:
                TryPickupMilkFromPen();
                break;

            case ZoneType.SaleDelivery:
                if (inventory.SoupCount > 0)
                    TryDepositSoupToSale();
                if (inventory.MilkCount > 0)
                    TryDepositMilkToSale();
                break;

            case ZoneType.MoneyPickup:
                TryPickupMoney();
                break;

            case ZoneType.ToolUpgrade:
                HandleMoneyDeposit(
                    zone.GetComponentInParent<ToolUpgradeController>());
                break;

            case ZoneType.FarmerHire:
                HandleMoneyDeposit(
                    zone.GetComponentInParent<FarmerHireController>());
                break;

            case ZoneType.SoupCourierHire:
                HandleMoneyDeposit(
                    zone.GetComponentInParent<CourierHireController>());
                break;

            case ZoneType.PenUpgrade:
                HandleMoneyDeposit(
                    zone.GetComponentInParent<PenExpansionController>());
                break;

            case ZoneType.PenFinalUpgrade:
                HandleMoneyDeposit(
                    zone.GetComponentInParent<PenFinalExpansionController>());
                break;
        }
    }

    // 공통 픽업 로직
    private void TryPickupItem(
        IPickupSource source,
        System.Func<bool> isFull,
        System.Func<int, bool> addToInventory)
    {
        if (source == null) return;

        pickupTimer += Time.deltaTime;
        if (pickupTimer < pickupInterval) return;
        pickupTimer = 0f;

        if (isFull())
        {
            maxUIController?.Show();
            return;
        }

        if (!source.HasItem(1)) return;

        bool added = addToInventory(1);
        if (!added)
        {
            maxUIController?.Show();
            return;
        }

        source.TryTakeItem(1);
        Sfx.PlayAtPoint(SoundId.ItemMove, transform.position);
    }

    // 공통 납품 로직
    private void TryDepositItem(
        IItemDepositTarget target,
        System.Func<bool> hasItem,
        System.Func<int, bool> takeFromInventory,
        System.Action onRollBack)
    {
        if (target == null) return;

        depositTimer += Time.deltaTime;
        if (depositTimer < depositInterval) return;
        depositTimer = 0f;

        if (!hasItem()) return;
        if (!target.CanAddItem(1)) return;

        bool took = takeFromInventory(1);
        if (!took) return;

        bool added = target.TryAddItem(1);
        if (!added)
        {
            onRollBack();
            return;
        }

        Sfx.PlayAtPoint(SoundId.ItemMove, transform.position);
    }

    // 공통 지불 로직 — ToolUpgrade / FarmerHire / CourierHire / PenUpgrade
    private void HandleMoneyDeposit(IMoneyDepositTarget target)
    {
        if (target == null || target.IsCompleted) return;

        moneyDepositTimer += Time.deltaTime;
        if (moneyDepositTimer < depositInterval) return;
        moneyDepositTimer = 0f;

        if (inventory.MoneyCount <= 0) return;
        if (MoneyManager.Instance == null) return;

        int actualPay = Mathf.Min(moneyValuePerDeposit, Mathf.Min(inventory.MoneyCount, target.RemainingCost));
        if (actualPay <= 0) return;

        if (!inventory.TryTakeMoney(actualPay)) return;

        if (!MoneyManager.Instance.TrySpendMoney(actualPay))
        {
            inventory.TryAddMoney(actualPay); // 롤백
            return;
        }

        target.DepositMoney(actualPay);
        Sfx.Play(SoundId.MoneySpend);

        // 포물선 이펙트: 플레이어 머리 -> zone
        if (moneyFlyController != null && playerHeadPoint != null)
        {
            Vector3 zonePos = zoneDetector.CurrentZone.transform.position + Vector3.up * 0.5f;
            moneyFlyController.PlaySpend(playerHeadPoint.position, zonePos);
        }
    }

    // 존별 픽업 / 납품

    private void TryDepositCarrotToCooking() =>
        TryDepositItem(
            cookingMachineController,
            () => inventory.CarrotCount > 0,
            amount => inventory.TryTakeCarrot(amount),
            () => inventory.TryAddCarrot(1));

    private void TryPickupSoupFromCooking() =>
        TryPickupItem(
            cookingMachineController,
            () => inventory.IsSoupFull,
            amount => inventory.TryAddSoup(amount));

    private void TryDepositSoupToSale() =>
        TryDepositItem(
            new SaleDeskDepositAdapter(saleDeskController, SaleDeskDepositAdapter.TargetType.Soup),
            () => inventory.SoupCount > 0,
            amount => inventory.TryTakeSoup(amount),
            () => inventory.TryAddSoup(1));

    private void TryPickupMilkFromPen() =>
        TryPickupItem(
            penCollectionBox,
            () => inventory.IsMilkFull,
            amount => inventory.TryAddMilk(amount));

    private void TryDepositMilkToSale() =>
        TryDepositItem(
            new SaleDeskDepositAdapter(saleDeskController, SaleDeskDepositAdapter.TargetType.Milk),
            () => inventory.MilkCount > 0,
            amount => inventory.TryTakeMilk(amount),
            () => inventory.TryAddMilk(1));

    private void TryPickupMoney()
    {
        if (saleDeskController == null) return;

        moneyPickupTimer += Time.deltaTime;
        if (moneyPickupTimer < pickupInterval) return;
        moneyPickupTimer = 0f;

        if (!saleDeskController.TryTakeMoney()) return;

        inventory.TryAddMoney(moneyValuePerPickup);
        MoneyManager.Instance.AddMoney(moneyValuePerPickup);

        // 포물선 이펙트: money zone -> 플레이어 머리
        if (moneyFlyController != null && playerHeadPoint != null)
        {
            Vector3 zonePos = zoneDetector.CurrentZone.transform.position + Vector3.up * 0.5f;
            moneyFlyController.PlayGain(zonePos, playerHeadPoint.position);
        }

        if (!hasNotifiedFirstMoney)
        {
            hasNotifiedFirstMoney = true;
            toolUpgradeRevealTrigger?.NotifyFirstMoneyAcquired();
        }

        Sfx.Play(SoundId.MoneyPickup);
    }
}

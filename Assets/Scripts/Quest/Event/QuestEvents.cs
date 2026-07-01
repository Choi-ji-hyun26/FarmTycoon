/// <summary>
/// 퀘스트 조건 감지용 이벤트 페이로드 구조체 모음
/// EventBus.Publish(payload) 형태로 발행
/// </summary>

// 당근 수확 (PlayerInventory.TryAddCarrot)
public struct CarrotHarvestedEvent
{
    public int count;
}

// 수프 생산 (CookingMachineController.ConsumeCarrotAndProduceSoup)
public struct SoupProducedEvent
{
    public int count;
}

// 수프 판매 (SaleDeskController.TrySupplyCustomer)
public struct SoupSoldEvent
{
    public int count;
}

// 우유 판매 (SaleDeskController.TrySupplyCustomer)
public struct MilkSoldEvent
{
    public int count;
}

// 손님 응대 완료 (CustomerServedCounter.NotifyServed)
public struct CustomerServedEvent
{
    public int totalCount;
}

// 도구 업그레이드 완료 (ToolUpgradeController.CompleteUpgradeRoutine)
// tier로 어떤 도구가 업그레이드됐는지 식별
public struct ToolUpgradedEvent
{
    public FarmingToolTier tier;
}

// 농부 고용 완료 (FarmerHireController.CompleteUpgradeRoutine)
// 대상이 하나뿐이라 식별자 불필요
public struct FarmerHiredEvent { }

// 배달원 고용 완료 (CourierHireController.CompleteUpgradeRoutine)
// 대상이 하나뿐이라 식별자 불필요
public struct CourierHiredEvent { }

// 동물 우리 확장 완료 (PenExpansionController.CompleteUpgradeRoutine)
// stage로 몇 차 확장인지 식별
public struct PenExpandedEvent
{
    public PenExpansionStage stage;
}

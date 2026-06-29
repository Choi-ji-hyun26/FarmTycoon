/// <summary>
/// 퀘스트 조건 감지용 이벤트 페이로드 구조체 모음
/// EventBus<T>.Publish(payload) 형태로 발행
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
public struct ToolUpgradedEvent { }

// 농부 고용 완료 (FarmerHireController.CompleteUpgradeRoutine)
public struct FarmerHiredEvent { }

// 배달원 고용 완료 (CourierHireController.CompleteUpgradeRoutine)
public struct CourierHiredEvent { }

// 동물 우리 확장 완료 (PenExpansionController.CompleteUpgradeRoutine)
public struct PenExpandedEvent { }
/*
역할
픽업 가능한 소스 오브젝트가 구현해야 하는 인터페이스
CookingMachineController, PenCollectionBox 등이 구현
*/

public interface IPickupSource
{
    int AvailableCount { get; }
    bool HasItem(int amount = 1);
    bool TryTakeItem(int amount = 1);
}

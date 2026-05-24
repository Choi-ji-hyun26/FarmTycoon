/*
역할
아이템을 받을 수 있는 대상이 구현해야 하는 인터페이스
SaleDesk, CustomerDesk 등이 구현
*/

public interface IItemDepositTarget
{
    bool CanAddItem(int amount = 1);
    bool TryAddItem(int amount = 1);
}

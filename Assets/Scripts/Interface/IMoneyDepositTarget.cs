public interface IMoneyDepositTarget
{
    bool IsCompleted { get; }
    int RemainingCost { get; }
    void DepositMoney(int amount);
}
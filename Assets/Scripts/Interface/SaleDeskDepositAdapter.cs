/*
역할
SaleDeskController의 수프/우유 납품을 IItemDepositTarget으로 감쌈
아이템 타입에 따라 적절한 납품 메서드 호출
*/
public class SaleDeskDepositAdapter : IItemDepositTarget
{
    public enum TargetType { Soup, Milk }

    private readonly SaleDeskController desk;
    private readonly TargetType type;

    public SaleDeskDepositAdapter(SaleDeskController desk, TargetType type)
    {
        this.desk = desk;
        this.type = type;
    }

    public bool CanAddItem(int amount = 1) => type == TargetType.Soup
        ? desk.CanAddSoup(amount)
        : desk.CanAddMilk(amount);

    public bool TryAddItem(int amount = 1) => type == TargetType.Soup
        ? desk.TryAddSoup(amount)
        : desk.TryAddMilk(amount);
}
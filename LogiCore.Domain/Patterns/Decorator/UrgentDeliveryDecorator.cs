namespace LogiCore.Domain.Patterns.Decorator;

public class UrgentDeliveryDecorator : DeliveryCostDecorator
{
    public UrgentDeliveryDecorator(IDeliveryCost innerCost) : base(innerCost) { }

    public override decimal Total => base.Total * 1.15m; // +15% за приоритет
    public override string Describe() => $"{base.Describe()} + срочная подача (+15%)";
}
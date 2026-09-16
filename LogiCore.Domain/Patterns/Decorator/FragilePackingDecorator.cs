namespace LogiCore.Domain.Patterns.Decorator;

public class FragilePackingDecorator : DeliveryCostDecorator
{
    private readonly decimal _flatPackingFee;

    public FragilePackingDecorator(IDeliveryCost innerCost, decimal flatPackingFee = 1500m) 
        : base(innerCost)
    {
        _flatPackingFee = flatPackingFee;
    }

    public override decimal Total => base.Total + _flatPackingFee;
    public override string Describe() => $"{base.Describe()} + упаковка хрупких грузов: {_flatPackingFee:C}";
}
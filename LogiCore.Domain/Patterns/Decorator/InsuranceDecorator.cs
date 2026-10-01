namespace LogiCore.Domain.Patterns.Decorator;

public class InsuranceDecorator : DeliveryCostDecorator
{
    private readonly decimal _insuranceFee;

    public InsuranceDecorator(IDeliveryCost innerCost, decimal declaredValue, decimal insuranceRate = 0.02m) 
        : base(innerCost)
    {
        _insuranceFee = declaredValue * insuranceRate;
    }

    public override decimal Total => base.Total + _insuranceFee;
    public override string Describe() => $"{base.Describe()} + страховка: {_insuranceFee:C}";
}
namespace LogiCore.Domain.Patterns.Decorator;

public class BaseDeliveryCost : IDeliveryCost
{
    public decimal Total { get; }

    public BaseDeliveryCost(decimal baseAmount)
    {
        Total = baseAmount;
    }

    public string Describe() => $"Базовая стоимость: {Total:C}";
}
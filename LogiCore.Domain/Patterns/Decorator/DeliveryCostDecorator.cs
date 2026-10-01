namespace LogiCore.Domain.Patterns.Decorator;

public abstract class DeliveryCostDecorator : IDeliveryCost
{
    protected readonly IDeliveryCost _innerCost;

    protected DeliveryCostDecorator(IDeliveryCost innerCost)
    {
        _innerCost = innerCost ?? throw new ArgumentNullException(nameof(innerCost));
    }

    public virtual decimal Total => _innerCost.Total;
    public virtual string Describe() => _innerCost.Describe();
}
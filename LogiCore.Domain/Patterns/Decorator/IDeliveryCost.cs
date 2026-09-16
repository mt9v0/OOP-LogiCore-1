namespace LogiCore.Domain.Patterns.Decorator;

public interface IDeliveryCost
{
    decimal Total { get; }
    string Describe();
}
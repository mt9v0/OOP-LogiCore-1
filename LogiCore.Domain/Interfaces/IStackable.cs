namespace LogiCore.Domain.Interfaces;

public interface IStackable
{
    bool CanBeStacked { get; }
    int MaxStackHeight { get; }
}
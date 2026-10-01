namespace LogiCore.Domain.Enums;

[Flags]
public enum TransportConditions
{
    None = 0,
    Refrigerated = 1 << 0,
    Sealed = 1 << 1,
    Pressurized = 1 << 2,
    LongRange = 1 << 3
}
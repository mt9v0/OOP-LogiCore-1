namespace LogiCore.Domain.Patterns.Singleton;

public sealed class TariffConfig
{
    private static readonly Lazy<TariffConfig> _instance = new(() => new TariffConfig());

    public static TariffConfig Instance => _instance.Value;

    public decimal BaseDiscountRate { get; set; } = 0.05m;
    public decimal DefaultInsuranceRate { get; set; } = 0.02m;

    private TariffConfig() { }
}
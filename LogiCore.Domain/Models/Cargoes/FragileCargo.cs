namespace LogiCore.Domain.Models.Cargoes;

public class FragileCargo : Cargo
{
    public double RiskFactor { get; }

    public FragileCargo(string description, double weightKg, double volumeM3, decimal declaredValue, double riskFactor = 1.5)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        if (riskFactor <= 0)
        throw new ArgumentException("RiskFactor должен быть больше 0", nameof(riskFactor));
        RiskFactor = riskFactor;
    }
}

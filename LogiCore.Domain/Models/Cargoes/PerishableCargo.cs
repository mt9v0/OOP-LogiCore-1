namespace LogiCore.Domain.Models.Cargoes;

using LogiCore.Domain.Interfaces;

public class PerishableCargo : Cargo, ITemperatureSensitive
{
    public DateTime ExpirationDate { get; }
    public double RequiredTemperatureC { get; }

    public PerishableCargo(string description, double weightKg, double volumeM3, decimal declaredValue, 
                           DateTime expirationDate, double requiredTemperatureC)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        ExpirationDate = expirationDate;
        RequiredTemperatureC = requiredTemperatureC;
    }
}

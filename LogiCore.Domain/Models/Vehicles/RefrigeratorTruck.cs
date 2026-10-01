using LogiCore.Domain.Interfaces;
using LogiCore.Domain.Models.Cargoes;

namespace LogiCore.Domain.Models.Vehicles;

public class RefrigeratorTruck : Truck
{
    public double MinTemperatureC { get; }
    public double MaxTemperatureC { get; }

    public RefrigeratorTruck(string registrationNumber, double maxLoadKg, double maxVolumeM3, 
                             double averageSpeedKmH, decimal baseRatePerKm, 
                             double minTemperatureC, double maxTemperatureC, decimal tollRoadFactor = 1.2m)
        : base(registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm, tollRoadFactor)
    {
        if (minTemperatureC > maxTemperatureC)
            throw new ArgumentException("Минимальная температура не может быть больше максимальной.....");

        MinTemperatureC = minTemperatureC;
        MaxTemperatureC = maxTemperatureC;
    }

    public override bool CanCarry(Cargo cargo)
    {
        if (!base.CanCarry(cargo)) return false;
        if (cargo is ITemperatureSensitive tempCargo)
        {
            return tempCargo.RequiredTemperatureC >= MinTemperatureC && 
                   tempCargo.RequiredTemperatureC <= MaxTemperatureC;
        }
        return true;
    }
}

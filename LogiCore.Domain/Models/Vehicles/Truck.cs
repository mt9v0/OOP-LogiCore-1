using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

namespace LogiCore.Domain.Models.Vehicles;

public class Truck : Vehicle
{
    public decimal TollRoadFactor { get; }

    public Truck(string registrationNumber, double maxLoadKg, double maxVolumeM3, 
                 double averageSpeedKmH, decimal baseRatePerKm, decimal tollRoadFactor = 1.15m)
        : base(registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm)
    {
        if (tollRoadFactor < 1.0m)
            throw new ArgumentException("Коэффициент платных дорог не может быть меньше 1", nameof(tollRoadFactor));
        TollRoadFactor = tollRoadFactor;
    }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo)
    {
        ArgumentNullException.ThrowIfNull(route);
        return (decimal)route.DistanceKm * BaseRatePerKm * TollRoadFactor;
    }
}

using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

namespace LogiCore.Domain.Models.Vehicles;

public class CargoShip : Vehicle
{
    public decimal OversizedSurchargeRate { get; }

    public CargoShip(string registrationNumber, double maxLoadKg, double maxVolumeM3, 
                     double averageSpeedKmH, decimal baseRatePerKm, decimal oversizedSurchargeRate = 0.25m)
        : base(registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm)
    {
        OversizedSurchargeRate = oversizedSurchargeRate;
    }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(cargo);
        decimal cost = (decimal)route.DistanceKm * BaseRatePerKm;
        if (cargo.Any(c => c is OversizedCargo))
        {
            cost += cost * OversizedSurchargeRate;
        }
        return cost;
    }
}

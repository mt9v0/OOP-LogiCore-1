using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

namespace LogiCore.Domain.Models.Vehicles;

public class CargoPlane : Vehicle
{
    public decimal SurchargePerKg { get; }

    public CargoPlane(string registrationNumber, double maxLoadKg, double maxVolumeM3, 
                      double averageSpeedKmH, decimal baseRatePerKm, decimal surchargePerKg = 15m)
        : base(registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm)
    {
        SurchargePerKg = surchargePerKg;
    }

    public override bool CanCarry(Cargo cargo)
    {
        if (!base.CanCarry(cargo)) return false;
        if (cargo is DangerousCargo dangerous && dangerous.DangerClass <= 3) return false;
        return true;
    }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(cargo);
        decimal distanceCost = (decimal)route.DistanceKm * BaseRatePerKm;
        double totalWeight = cargo.Sum(c => c.WeightKg);
        decimal weightSurcharge = (decimal)totalWeight * SurchargePerKg;
        return distanceCost + weightSurcharge;
    }
}

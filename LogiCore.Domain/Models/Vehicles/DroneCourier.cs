using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

namespace LogiCore.Domain.Models.Vehicles;

public sealed class DroneCourier : Vehicle
{
    public double MaxFlightRangeKm { get; }

    public DroneCourier(string registrationNumber, double maxLoadKg, double maxVolumeM3, 
                        double averageSpeedKmH, decimal baseRatePerKm, double maxFlightRangeKm = 30.0)
        : base(registrationNumber, maxLoadKg, maxVolumeM3, averageSpeedKmH, baseRatePerKm)
    {
        MaxFlightRangeKm = maxFlightRangeKm;
    }

    public override bool CanCarry(Cargo cargo)
    {
        if (!base.CanCarry(cargo)) return false;
        if (cargo is DangerousCargo or OversizedCargo) return false;
        return true;
    }

    public override decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo)
    {
        ArgumentNullException.ThrowIfNull(route);
        if (route.DistanceKm > MaxFlightRangeKm)
        {

            throw new InvalidOperationException($"Дальность маршрута ({route.DistanceKm} км) превышает радиус полёта ({MaxFlightRangeKm} км)");
        }
        return (decimal)route.DistanceKm * BaseRatePerKm;
    }
}

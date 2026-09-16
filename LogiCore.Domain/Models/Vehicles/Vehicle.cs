using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Interfaces;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

namespace LogiCore.Domain.Models.Vehicles;

[JsonDerivedType(typeof(Truck), "фура")]
[JsonDerivedType(typeof(RefrigeratorTruck), "рефриджератор")]
[JsonDerivedType(typeof(CargoPlane), "самолет")]
[JsonDerivedType(typeof(CargoShip), "корабль")]
[JsonDerivedType(typeof(DroneCourier), "дрон")]
public abstract class Vehicle : IEntity, IEquatable<Vehicle>
{
    private static readonly Regex ValidNumberPattern =
        new(@"^[A-ZА-Я0-9\-]{4,15}$", RegexOptions.Compiled);

    public Guid Id { get; }
    public string RegistrationNumber { get; }
    public double MaxLoadKg { get; }
    public double MaxVolumeM3 { get; }
    public double AverageSpeedKmH { get; }
    public decimal BaseRatePerKm { get; }
    public VehicleState State { get; private set; }

    private static readonly Dictionary<VehicleState, VehicleState[]> AllowedTransitions = new()
    {
        [VehicleState.Free] = new[] { VehicleState.Assigned },
        [VehicleState.Assigned] = new[] { VehicleState.InTransit, VehicleState.Free },
        [VehicleState.InTransit] = new[] { VehicleState.Free },
    };

    protected Vehicle(string registrationNumber, double maxLoadKg, double maxVolumeM3, 
                      double averageSpeedKmH, decimal baseRatePerKm)
    {
        ValidateRegistrationNumber(registrationNumber);
        if (maxLoadKg <= 0)
            throw new ArgumentException("Грузоподъемность должна быть строго больше 0.", nameof(maxLoadKg));
        if (maxVolumeM3 <= 0)
            throw new ArgumentException("Объем кузова должен быть строго больше 0.", nameof(maxVolumeM3));
        if (averageSpeedKmH <= 0)
            throw new ArgumentException("Скорость должна быть строго больше 0.", nameof(averageSpeedKmH));
        if (baseRatePerKm < 0)
            throw new ArgumentException("Базовый тариф не может быть отрицательным.", nameof(baseRatePerKm));

        Id = Guid.NewGuid();
        RegistrationNumber = registrationNumber.Trim().ToUpperInvariant();
        MaxLoadKg = maxLoadKg;
        MaxVolumeM3 = maxVolumeM3;
        AverageSpeedKmH = averageSpeedKmH;
        BaseRatePerKm = baseRatePerKm;
        State = VehicleState.Free;
    }

    private static void ValidateRegistrationNumber(string registrationNumber)
    {
        var trimmed = registrationNumber?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Госномер не может быть пустым.", nameof(registrationNumber));

        var upper = trimmed.ToUpperInvariant();

        if (!ValidNumberPattern.IsMatch(upper))
            throw new ArgumentException(
                $"Некорректный формат номера: '{registrationNumber}'.", nameof(registrationNumber));

        if (!upper.Any(char.IsDigit))
            throw new ArgumentException(
                $"Номер должен содержать хотя бы одну цифру: '{registrationNumber}'.", nameof(registrationNumber));

        if (upper.Distinct().Count() == 1)
            throw new ArgumentException(
                $"Номер выглядит некорректным: '{registrationNumber}'.", nameof(registrationNumber));
    }

    internal void SetState(VehicleState newState)
    {
        if (!AllowedTransitions.TryGetValue(State, out var allowed) || !allowed.Contains(newState))
        {
            throw new InvalidOperationException(
                $"Недопустимый переход состояния ТС: {State} -> {newState}.");
        }

        State = newState;
    }

    public virtual bool CanCarry(Cargo cargo)
    {
        if (cargo == null) return false;
        return cargo.WeightKg <= MaxLoadKg && cargo.VolumeM3 <= MaxVolumeM3;
    }

    public abstract decimal CalculateDeliveryCost(Route route, IReadOnlyCollection<Cargo> cargo);

    public override bool Equals(object? obj) => Equals(obj as Vehicle);
    public bool Equals(Vehicle? other) => other != null && (ReferenceEquals(this, other) || Id.Equals(other.Id));
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"[{GetType().Name}] Рег.№: {RegistrationNumber}, Статус: {State}, Нагрузка: {MaxLoadKg}кг/{MaxVolumeM3}м^3";
}

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

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
    [JsonInclude]
    public Guid Id { get; private set; }
    public string RegistrationNumber { get; }
    public double MaxLoadKg { get; }
    public double MaxVolumeM3 { get; }
    public double AverageSpeedKmH { get; }
    public decimal BaseRatePerKm { get; }
    [JsonInclude]
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

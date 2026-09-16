namespace LogiCore.Domain.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Vehicles;

public class CargoCompatibilityValidator
{
    public void ValidateCargoesOnly(IReadOnlyCollection<Cargo> cargoes)
    {
        ArgumentNullException.ThrowIfNull(cargoes);

        if (cargoes.Count == 0)
            throw new CargoValidationException("Партия грузов не может быть пустой :0");

        ValidateExpirationDates(cargoes);
        ValidateCargoIntercompatibility(cargoes);
    }

    public void Validate(Vehicle vehicle, IReadOnlyCollection<Cargo> cargoes)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ValidateCargoesOnly(cargoes);

        ValidateTotalCapacity(vehicle, cargoes);
        ValidateVehicleCapabilities(vehicle, cargoes);
    }

    private static void ValidateExpirationDates(IEnumerable<Cargo> cargoes)
    {
        foreach (var cargo in cargoes)
        {
            if (cargo is PerishableCargo perishable && perishable.ExpirationDate < DateTime.UtcNow)
            {
                throw new CargoValidationException(
                    $"Груз '{perishable.Description}' просрочен!! срок годности до {perishable.ExpirationDate:d})");
            }
        }
    }

    private static void ValidateCargoIntercompatibility(IReadOnlyCollection<Cargo> cargoes)
    {
        bool hasDangerous = cargoes.Any(c => c is DangerousCargo);
        bool hasPerishable = cargoes.Any(c => c is PerishableCargo); 
        if (hasDangerous && hasPerishable)
        {
            throw new IncompatibleCargoException(
                "Запрещено перевозить опасные грузы совместно со скоропортящимися в одном ТС");
        }
    }

    private static void ValidateVehicleCapabilities(Vehicle vehicle, IEnumerable<Cargo> cargoes)
    {
        foreach (var cargo in cargoes)
        {
            if (!vehicle.CanCarry(cargo))
            {
                throw new IncompatibleCargoException(
                    $"Транспорт {vehicle.RegistrationNumber} ({vehicle.GetType().Name}) " +
                    $"не подходит для перевозки груза '{cargo.Description}'");
            }
        }
    }

    private static void ValidateTotalCapacity(Vehicle vehicle, IReadOnlyCollection<Cargo> cargoes)
    {
        double totalWeight = cargoes.Sum(c => c.WeightKg);
        if (totalWeight > vehicle.MaxLoadKg)
        {
            throw new VehicleOverloadException(
                $"Превышена грузоподъемность ТС {vehicle.RegistrationNumber}. " +
                $"Суммарный вес: {totalWeight} кг, Лимит: {vehicle.MaxLoadKg} кг!",
                totalWeight, vehicle.MaxLoadKg);
        }

        double totalVolume = cargoes.Sum(c => c.VolumeM3);
        if (totalVolume > vehicle.MaxVolumeM3)
        {
            throw new VehicleOverloadException(
                $"Превышен доступный объем ТС {vehicle.RegistrationNumber}. " +
                $"Суммарный объем: {totalVolume} м^3, Лимит: {vehicle.MaxVolumeM3} м^3.",
                totalVolume, vehicle.MaxVolumeM3);
        }
    }
}

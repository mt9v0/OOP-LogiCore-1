namespace LogiCore.Domain.Patterns.Factory;

using LogiCore.Domain.Models.Vehicles;

public static class VehicleFactory
{
    public static Vehicle CreateVehicle(string type, string regNum, double maxLoadKg, double maxVolumeM3, double speed, decimal rate)
    {
        return type.ToLowerInvariant() switch
        {
            "truck" or "Фура" => new Truck(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            "refrigerator" or "Рефрижератор" => new RefrigeratorTruck(regNum, maxLoadKg, maxVolumeM3, speed, rate, -20, 5),
            "plane" or "Самолет" => new CargoPlane(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            "ship" or "Корабль" => new CargoShip(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            "drone" or "Дрон" => new DroneCourier(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            _ => throw new ArgumentException($"Неизвестный тип транспорта: {type}")
        };
    }
}
namespace LogiCore.Domain.Patterns.Factory;

using LogiCore.Domain.Models.Vehicles;

public static class VehicleFactory
{
    public static Vehicle CreateVehicle(string type, string regNum, double maxLoadKg, double maxVolumeM3, double speed, decimal rate)
    {
        return type.ToLowerInvariant() switch
        {
            "truck" or "фура" => new Truck(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            "refrigerator" or "рефрижератор" => new RefrigeratorTruck(regNum, maxLoadKg, maxVolumeM3, speed, rate, -20, 5),
            "plane" or "самолет" => new CargoPlane(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            "ship" or "корабль" => new CargoShip(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            "drone" or "дрон" => new DroneCourier(regNum, maxLoadKg, maxVolumeM3, speed, rate),
            _ => throw new ArgumentException($"Неизвестный тип транспорта: {type}")
        };
    }
}
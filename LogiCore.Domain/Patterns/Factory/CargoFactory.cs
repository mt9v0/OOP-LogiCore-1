namespace LogiCore.Domain.Patterns.Factory;

using LogiCore.Domain.Models.Cargoes;

public static class CargoFactory
{
    public static Cargo CreateStandard(string desc, double weight, double volume, decimal price) =>
        new StandardCargo(desc, weight, volume, price);

    public static Cargo CreatePerishable(string desc, double weight, double volume, decimal price, DateTime expDate, double temp) =>
        new PerishableCargo(desc, weight, volume, price, expDate, temp);

    public static Cargo CreateFragile(string desc, double weight, double volume, decimal price, double risk = 1.5) =>
        new FragileCargo(desc, weight, volume, price, risk);

    public static Cargo CreateDangerous(string desc, double weight, double volume, decimal price, int dangerClass) =>
        new DangerousCargo(desc, weight, volume, price, dangerClass);

    public static Cargo CreateOversized(string desc, double weight, double volume, decimal price, double extraLength) =>
        new OversizedCargo(desc, weight, volume, price, extraLength);
}
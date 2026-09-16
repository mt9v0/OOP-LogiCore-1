namespace LogiCore.Domain.Models.Cargoes;

using System.Text.Json.Serialization;
using LogiCore.Domain.Interfaces;

[JsonDerivedType(typeof(StandardCargo), "standard")]
[JsonDerivedType(typeof(PerishableCargo), "perishable")]
[JsonDerivedType(typeof(FragileCargo), "fragile")]
[JsonDerivedType(typeof(DangerousCargo), "dangerous")]
[JsonDerivedType(typeof(OversizedCargo), "oversized")]
public abstract class Cargo : IEntity, IInsurable
{
    public Guid Id { get; }
    public string Description { get; }
    public double WeightKg { get; }
    public double VolumeM3 { get; }
    public decimal DeclaredValue { get; }

    protected Cargo(string description, double weightKg, double volumeM3, decimal declaredValue)
    {
        if (weightKg <= 0)
            throw new ArgumentException("Вес груза должен быть больше 0", nameof(weightKg));
        if (volumeM3 <= 0)
            throw new ArgumentException("Объём груза должен быть больше 0", nameof(volumeM3));
        if (declaredValue < 0)
            throw new ArgumentException("Объявленная стоимость не может быть отрицательной!!", nameof(declaredValue));

        Id = Guid.NewGuid();
        Description = description ?? throw new ArgumentNullException(nameof(description));
        WeightKg = weightKg;
        VolumeM3 = volumeM3;
        DeclaredValue = declaredValue;
    }
}

public class StandardCargo : Cargo, IStackable
{
    public StandardCargo(string description, double weightKg, double volumeM3, decimal declaredValue)
        : base(description, weightKg, volumeM3, declaredValue) { }

    bool IStackable.CanBeStacked => true;
    int IStackable.MaxStackHeight => 3;
}

public class PerishableCargo : Cargo, ITemperatureSensitive
{
    public DateTime ExpirationDate { get; }
    public double RequiredTemperatureC { get; }

    public PerishableCargo(string description, double weightKg, double volumeM3, decimal declaredValue, 
                           DateTime expirationDate, double requiredTemperatureC)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        ExpirationDate = expirationDate;
        RequiredTemperatureC = requiredTemperatureC;
    }
}

public class FragileCargo : Cargo
{
    public double RiskFactor { get; }

    public FragileCargo(string description, double weightKg, double volumeM3, decimal declaredValue, double riskFactor = 1.5)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        RiskFactor = riskFactor;
    }
}

public class DangerousCargo : Cargo
{
    public int DangerClass { get; }

    public DangerousCargo(string description, double weightKg, double volumeM3, decimal declaredValue, int dangerClass)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        if (dangerClass is < 1 or > 9)
            throw new ArgumentOutOfRangeException(nameof(dangerClass), "Класс опасности должен быть от 1 до 9");
        DangerClass = dangerClass;
    }
}

public class OversizedCargo : Cargo
{
    public double ExtraLengthM { get; }

    public OversizedCargo(string description, double weightKg, double volumeM3, decimal declaredValue, double extraLengthM)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        ExtraLengthM = extraLengthM;
    }
}
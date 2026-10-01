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
    [JsonInclude]
    public Guid Id { get; private set; }
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

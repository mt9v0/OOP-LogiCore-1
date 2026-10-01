namespace LogiCore.Domain.Models.Cargoes;

public class OversizedCargo : Cargo
{
    public double ExtraLengthM { get; }

    public OversizedCargo(string description, double weightKg, double volumeM3, decimal declaredValue, double extraLengthM)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        if (extraLengthM <= 0)
        throw new ArgumentException("ExtraLengthM должен быть больше 0", nameof(extraLengthM));
        ExtraLengthM = extraLengthM;
    }
}
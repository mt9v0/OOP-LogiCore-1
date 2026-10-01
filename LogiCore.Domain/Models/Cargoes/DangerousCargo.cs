namespace LogiCore.Domain.Models.Cargoes;

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

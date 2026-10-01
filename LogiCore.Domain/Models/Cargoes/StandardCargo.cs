namespace LogiCore.Domain.Models.Cargoes;

using LogiCore.Domain.Interfaces;

public class StandardCargo : Cargo, IStackable
{
    public StandardCargo(string description, double weightKg, double volumeM3, decimal declaredValue)
        : base(description, weightKg, volumeM3, declaredValue) { }

    bool IStackable.CanBeStacked => true;
    int IStackable.MaxStackHeight => 3;
}
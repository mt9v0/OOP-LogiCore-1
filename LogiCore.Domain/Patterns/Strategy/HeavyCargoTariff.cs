namespace LogiCore.Domain.Patterns.Strategy;

using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

public class HeavyCargoTariff : ITariffStrategy
{
    public string Name => "Тариф для тяжеловесных грузов";

    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargoes)
    {
        double totalWeight = cargoes.Sum(c => c.WeightKg);
        decimal weightMultiplier = totalWeight > 5000 ? 1.25m : 1.05m;
        return baseCost * weightMultiplier;
    }
}
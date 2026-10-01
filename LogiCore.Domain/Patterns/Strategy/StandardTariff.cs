namespace LogiCore.Domain.Patterns.Strategy;

using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

public class StandardTariff : ITariffStrategy
{
    public string Name => "Стандартный тариф";

    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargoes)
    {
        return baseCost;
    }
}
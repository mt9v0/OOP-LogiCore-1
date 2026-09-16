namespace LogiCore.Domain.Patterns.Strategy;

using System.Collections.Generic;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

public class ExpressTariff : ITariffStrategy
{
    public string Name => "Экспресс-тариф (+52% за скорость)";

    public decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargoes)
    {
        return baseCost * 1.40m;
    }
}
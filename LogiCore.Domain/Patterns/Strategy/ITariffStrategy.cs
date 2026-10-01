namespace LogiCore.Domain.Patterns.Strategy;

using System.Collections.Generic;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Routes;

public interface ITariffStrategy
{
    string Name { get; }
    decimal Calculate(decimal baseCost, Route route, IReadOnlyCollection<Cargo> cargoes);
}
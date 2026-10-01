namespace LogiCore.Domain.Models.Orders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Interfaces;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Customers;
using LogiCore.Domain.Models.Routes;
using LogiCore.Domain.Models.Vehicles;

public class Order : IEntity
{
    private readonly List<Cargo> _cargoes = new();

    [JsonInclude]
    public Guid Id { get; private set; }

    [JsonInclude]
    public Customer Customer { get; private set; } = null!;

    [JsonInclude]
    public Route Route { get; private set; } = null!;
    public IReadOnlyCollection<Cargo> Cargoes => _cargoes;

    [JsonInclude]
    public Vehicle? AssignedVehicle { get; private set; }

    [JsonInclude]
    public decimal FinalCost { get; private set; }

    [JsonInclude]
    public OrderStatus Status { get; private set; }

    [JsonConstructor]
    private Order(Guid id, Customer customer, Route route, IReadOnlyCollection<Cargo> cargoes,
                Vehicle? assignedVehicle, decimal finalCost, OrderStatus status)
    {
        Id = id;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Route = route ?? throw new ArgumentNullException(nameof(route));
        _cargoes = cargoes.ToList() ?? new List<Cargo>();
        AssignedVehicle = assignedVehicle;
        FinalCost = finalCost;
        Status = status;

        customer.AddOrder(this);  
    }

    public Order(Customer customer, Route route, IEnumerable<Cargo> cargoes)
    : this(Guid.NewGuid(), customer, route, ToListOrThrow(cargoes),
            null, 0m, OrderStatus.Created)
    {
    }

    private static List<Cargo> ToListOrThrow(IEnumerable<Cargo> cargoes)
    {
        var list = cargoes?.ToList() ?? throw new ArgumentNullException(nameof(cargoes));
        if (list.Count == 0)
            throw new ArgumentException("Заказ должен содержать хотя бы один груз", nameof(cargoes));
        return list;
    }

    public void AssignVehicle(Vehicle vehicle, decimal calculatedCost)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        if (calculatedCost < 0)
            throw new ArgumentOutOfRangeException(nameof(calculatedCost), "Стоимость не может быть отрицательной...");

        if (Status != OrderStatus.Created)
        {
            throw new InvalidOrderStateException(
                $"Невозможно назначить транспорт:( Текущий статус заказа: {Status} Ожидался: Created");
        }

        AssignedVehicle = vehicle;
        FinalCost = calculatedCost;
        Status = OrderStatus.Assigned;

        vehicle.SetState(VehicleState.Assigned);
    }

    public void StartDelivery()
    {
        if (Status != OrderStatus.Assigned)
        {
            throw new InvalidOrderStateException(
                $"Невозможно начать доставку:( Текущий статус заказа: {Status} Ожидался: Assigned");
        }

        Status = OrderStatus.InTransit;
        AssignedVehicle?.SetState(VehicleState.InTransit);
    }

    public void CompleteDelivery()
    {
        if (Status != OrderStatus.InTransit)
        {
            throw new InvalidOrderStateException(
                $"Невозможно завершить доставку:( Текущий статус заказа: {Status} Ожидался: InTransit");
        }

        Status = OrderStatus.Delivered;
        AssignedVehicle?.SetState(VehicleState.Free);
    }

    public void Cancel()
    {
  
        if (Status != OrderStatus.Created && Status != OrderStatus.Assigned)
        {
            throw new InvalidOrderStateException(
                $"Невозможно отменить заказ в статусе {Status}");
        }

        if (AssignedVehicle != null && AssignedVehicle.State != VehicleState.Free)
        {
            AssignedVehicle.SetState(VehicleState.Free);
        }

        Status = OrderStatus.Cancelled;
    }

    public override string ToString() =>
        $"Заказ [{Id.ToString()[..8]}] | Клиент: {Customer.Name} | Статус: {Status} | Стоимость: {FinalCost:C}";

    public void UpdateFinalCost(decimal newCost)
    {
        if (newCost < 0)
            throw new ArgumentOutOfRangeException(nameof(newCost), "Стоимость не может быть отрицательной");
        FinalCost = newCost;
    }
}


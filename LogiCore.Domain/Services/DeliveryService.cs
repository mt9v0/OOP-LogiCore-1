namespace LogiCore.Domain.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Events;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Customers;
using LogiCore.Domain.Models.Orders;
using LogiCore.Domain.Models.Routes;
using LogiCore.Domain.Models.Vehicles;

public class DeliveryService
{
    private readonly List<Vehicle> _vehicles = new();
    private readonly List<Order> _orders = new();
    private readonly CargoCompatibilityValidator _validator;

    public decimal TotalRevenue { get; private set; }
    public IReadOnlyCollection<Vehicle> Vehicles => _vehicles.AsReadOnly();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();


    public event EventHandler<OrderCreatedEventArgs>? OrderCreated;
    public event EventHandler<OrderStatusChangedEventArgs>? OrderStatusChanged;
    public event EventHandler<VehicleOverloadAttemptEventArgs>? VehicleOverloadAttempt;
    public event EventHandler<DeliveryCompletedEventArgs>? DeliveryCompleted;

    public event SystemLogHandler? SystemLogged;

    public DeliveryService(CargoCompatibilityValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public void RegisterVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        _vehicles.Add(vehicle);
    }

    public Order CreateAndProcessOrder(Customer customer, Route route, IReadOnlyCollection<Cargo> cargoes)
    {
        _validator.ValidateCargoesOnly(cargoes);

        var order = new Order(customer, route, cargoes);
        _orders.Add(order);

        OrderCreated?.Invoke(this, new OrderCreatedEventArgs(order));
        SystemLogged?.Invoke(this, $"Заказ №{order.Id.ToString()[..8]} создан!");

        var (bestVehicle, cost) = FindBestVehicle(route, cargoes);

        var oldStatus = order.Status;

        order.AssignVehicle(bestVehicle, cost);

        OrderStatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, oldStatus, order.Status));

        return order;
    }

    public (Vehicle Vehicle, decimal Cost) FindBestVehicle(Route route, IReadOnlyCollection<Cargo> cargoes)
    {
        Vehicle? bestVehicle = null;
        decimal minCost = decimal.MaxValue;

        var freeVehicles = _vehicles.Where(v => v.State == VehicleState.Free).ToList();

        foreach (var vehicle in freeVehicles)
        {
            try
            {
                _validator.Validate(vehicle, cargoes);
                decimal cost = vehicle.CalculateDeliveryCost(route, cargoes);

                if (cost < minCost)
                {
                    minCost = cost;
                    bestVehicle = vehicle;
                }
            }
            catch (VehicleOverloadException)
            {
                
            }
            catch (LogisticsException)
            {

            }
            catch (InvalidOperationException)
            {

            }
        }

        if (bestVehicle == null)
        {
            throw new RouteNotFoundException("Не удалось подобрать свободный транспорт для перевозки данной партии груза... анлак");
        }

        return (bestVehicle, minCost);
    }

    public void ReportOverloadAttempt(Vehicle vehicle, double attemptedWeight)
    {
        VehicleOverloadAttempt?.Invoke(this, new VehicleOverloadAttemptEventArgs(vehicle, attemptedWeight));
    }

    public void StartOrderDelivery(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureOrderIsManaged(order);

        var oldStatus = order.Status;
        order.StartDelivery();
        OrderStatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, oldStatus, order.Status));
    }

    public void CompleteOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        EnsureOrderIsManaged(order);

        var oldStatus = order.Status;
        order.CompleteDelivery();
        TotalRevenue += order.FinalCost;

        OrderStatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, oldStatus, order.Status));
        DeliveryCompleted?.Invoke(this, new DeliveryCompletedEventArgs(order));
    }

    private void EnsureOrderIsManaged(Order order)
    {
        if (!_orders.Contains(order))
        {
            throw new InvalidOperationException();
        }
    }
}

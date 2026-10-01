namespace LogiCore.Tests;

using System;
using System.Linq;

using LogiCore.Domain.Enums;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Models;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Customers;
using LogiCore.Domain.Models.Orders;
using LogiCore.Domain.Models.Routes;
using LogiCore.Domain.Models.Vehicles;
using LogiCore.Domain.Patterns.Decorator;
using LogiCore.Domain.Patterns.Factory;
using LogiCore.Domain.Repositories;
using LogiCore.Domain.Services;
using Xunit;

public class DomainTests
{
    private readonly Route _testRoute = new("МСК-СПБ", new[] { new RoutePoint(55.75, 37.61), new RoutePoint(59.93, 30.33) });
    private readonly Customer _testCustomer = new("дядя Вова", "+76660000000");

    [Fact]
    public void Truck_CalculateCost_AppliesTollRoadFactor()
    {
        var truck = new Truck("A101AA77", 5000, 30, 80, 50m, tollRoadFactor: 1.2m);
        var cargo = new[] { CargoFactory.CreateStandard("Коробки", 100, 1, 1000) };

        decimal cost = truck.CalculateDeliveryCost(_testRoute, cargo);

        Assert.True(cost > 0);
        Assert.Equal((decimal)_testRoute.DistanceKm * 50m * 1.2m, cost);
    }

    [Fact]
    public void DroneCourier_ExceedingMaxRange_ThrowsInvalidOperationException()
    {
        var drone = new DroneCourier("DRONE-1", 10, 0.1, 50, 100, maxFlightRangeKm: 5.0);
        var cargo = new[] { CargoFactory.CreateStandard("Документы", 1, 0.01, 500) };

        Assert.Throws<InvalidOperationException>(() => drone.CalculateDeliveryCost(_testRoute, cargo));
    }

    [Fact]
    public void CargoPlane_CalculatesSurchargeByWeight()
    {
        var plane = new CargoPlane("RA-101", 10000, 50, 500, 200m, surchargePerKg: 10m);
        var cargo = new[] { CargoFactory.CreateStandard("Оборудование", 500, 5, 50000) };

        decimal cost = plane.CalculateDeliveryCost(_testRoute, cargo);
        decimal expectedCost = ((decimal)_testRoute.DistanceKm * 200m) + (500m * 10m);

        Assert.Equal(expectedCost, cost);
    }

    [Fact]
    public void Validator_DangerousAndPerishableCargoTogether_ThrowsIncompatibleCargoException()
    {
        var validator = new CargoCompatibilityValidator();
        var truck = new Truck("A101AA77", 10000, 50, 80, 50m);
        var cargoes = new Cargo[]
        {
            CargoFactory.CreateDangerous("Химия", 100, 1, 5000, 2),
            CargoFactory.CreatePerishable("Продукты", 100, 1, 2000, DateTime.UtcNow.AddDays(2), 4)
        };

        Assert.Throws<IncompatibleCargoException>(() => validator.Validate(truck, cargoes));
    }

    [Fact]
    public void Validator_ExpiredCargo_ThrowsCargoValidationException()
    {
        var validator = new CargoCompatibilityValidator();
        var refr = new RefrigeratorTruck("B2B0000", 5000, 20, 70, 60m, -10, 10);
        var expiredCargo = new[] { CargoFactory.CreatePerishable("Молоко", 50, 0.5, 1000, DateTime.UtcNow.AddDays(-1), 4) };

        Assert.Throws<CargoValidationException>(() => validator.Validate(refr, expiredCargo));
    }

    [Fact]
    public void Validator_WeightOverload_ThrowsVehicleOverloadException()
    {
        var validator = new CargoCompatibilityValidator();
        var drone = new DroneCourier("DRONE-2", 10, 0.1, 50, 100);
        var heavyCargo = new[] { CargoFactory.CreateStandard("Гиря", 20, 0.05, 1000) };

        Assert.Throws<VehicleOverloadException>(() => validator.Validate(drone, heavyCargo));
    }

    [Fact]
    public void Validator_IncompatibleTemperature_ReturnsFalseOnCanCarry()
    {
        var refr = new RefrigeratorTruck("B067BB28", 5000, 20, 70, 60m, minTemperatureC: -20, maxTemperatureC: -10);
        var warmCargo = CargoFactory.CreatePerishable("Цветочки", 50, 0.5, 1000, DateTime.UtcNow.AddDays(5), +5);

        Assert.False(refr.CanCarry(warmCargo));
    }

    [Fact]
    public void Order_ValidStateTransitions_UpdatesStatusCorrectly()
    {
        var order = new Order(_testCustomer, _testRoute, new[] { CargoFactory.CreateStandard("Груз", 10, 0.1, 100) });
        var truck = new Truck("A128AA32", 5000, 30, 80, 50m);

        Assert.Equal(OrderStatus.Created, order.Status);

        order.AssignVehicle(truck, 1000m);
        Assert.Equal(OrderStatus.Assigned, order.Status);
        Assert.Equal(VehicleState.Assigned, truck.State);

        order.StartDelivery();
        Assert.Equal(OrderStatus.InTransit, order.Status);
        Assert.Equal(VehicleState.InTransit, truck.State);

        order.CompleteDelivery();
        Assert.Equal(OrderStatus.Delivered, order.Status);
        Assert.Equal(VehicleState.Free, truck.State);
    }

    [Fact]
    public void Order_InvalidTransition_ThrowsInvalidOrderStateException()
    {
        var order = new Order(_testCustomer, _testRoute, new[] { CargoFactory.CreateStandard("Груз", 10, 0.1, 100) });

        Assert.Throws<InvalidOrderStateException>(() => order.CompleteDelivery());
    }

    [Fact]
    public void Order_CancelDuringInTransit_ThrowsInvalidOrderStateException()
    {

        var order = new Order(_testCustomer, _testRoute, new[] { CargoFactory.CreateStandard("Груз", 10, 0.1, 100) });
        var truck = new Truck("A777AA01", 5000, 30, 80, 50m);

        order.AssignVehicle(truck, 1000m);
        order.StartDelivery();

        Assert.Equal(OrderStatus.InTransit, order.Status);
        Assert.Throws<InvalidOrderStateException>(() => order.Cancel());
    }

    [Fact]
    public void Repository_AddAndGetById_WorksCorrectly()
    {
        var repo = new Repository<Truck>();
        var truck = new Truck("A101AA77", 5000, 30, 80, 50m);

        repo.Add(truck);
        var fetched = repo.GetById(truck.Id);

        Assert.NotNull(fetched);
        Assert.Equal(truck.RegistrationNumber, fetched!.RegistrationNumber);
    }

    [Fact]
    public void Repository_YieldIteratorAndIndexer_IteratesAllItems()
    {
        var repo = new Repository<Truck>();
        var t1 = new Truck("A101AA77", 5000, 30, 80, 50m);
        var t2 = new Truck("B202BB77", 6000, 35, 80, 55m);

        repo.Add(t1);
        repo.Add(t2);

        int count = 0;
        foreach (var item in repo)
        {
            count++;
        }

        Assert.Equal(2, count);
        Assert.Equal(t1, repo[t1.Id]);
    }

    [Fact]
    public void Decorators_Composition_CalculatesCombinedPriceCorrectly()
    {
        IDeliveryCost cost = new BaseDeliveryCost(1000m);
        cost = new InsuranceDecorator(cost, declaredValue: 50000m, insuranceRate: 0.02m); // +1000
        cost = new FragilePackingDecorator(cost, flatPackingFee: 500m);                     // +500

        Assert.Equal(2500m, cost.Total);
        Assert.Contains("страховка", cost.Describe());
        Assert.Contains("упаковка", cost.Describe());
    }

    [Fact]
    public void UrgentDeliveryDecorator_AddsFifteenPercentSurcharge()
    {
        IDeliveryCost cost = new BaseDeliveryCost(1000m);
        cost = new UrgentDeliveryDecorator(cost);

        Assert.Equal(1150m, cost.Total);
    }

    [Fact]
    public void CreateAndProcessOrder_ReservesVehicle_PreventsDoubleAssignment()
    {
   
        var validator = new CargoCompatibilityValidator();
        var deliveryService = new DeliveryService(validator);
        var truck = new Truck("A101AA77", 5000, 30, 80, 50m);
        deliveryService.RegisterVehicle(truck);

        var cargo1 = new[] { CargoFactory.CreateStandard("Груз1", 100, 1, 1000) };
        var order1 = deliveryService.CreateAndProcessOrder(_testCustomer, _testRoute, cargo1);

        Assert.Equal(VehicleState.Assigned, truck.State);
        Assert.Equal(truck, order1.AssignedVehicle);

        var cargo2 = new[] { CargoFactory.CreateStandard("Груз200", 100, 1, 1000) };
        Assert.Throws<RouteNotFoundException>(() =>
            deliveryService.CreateAndProcessOrder(_testCustomer, _testRoute, cargo2));
    }

    [Fact]
    public void JsonSerialization_PolymorphicTypes_RestoresExactDerivedClasses()
    {
        var jsonStorage = new JsonStorageService();
        string tempFile = "test_state.json";

        var refr = new RefrigeratorTruck("B202BB77", 5000, 20, 70, 60m, -20, 5);
        var perishable = new PerishableCargo("Рыба", 200, 1, 5000, DateTime.UtcNow.AddDays(3), -18);
        var order = new Order(_testCustomer, _testRoute, new[] { perishable });
        order.AssignVehicle(refr, 5000m);

        var originalState = new SystemStateDto(
            new Vehicle[] { refr }.ToList(),
            new[] { _testCustomer }.ToList(),
            new[] { order }.ToList()
        );

        try
        {
            jsonStorage.SaveState(tempFile, originalState);
            var loadedState = jsonStorage.LoadState(tempFile);

            Assert.Single(loadedState.Vehicles);
            Assert.IsType<RefrigeratorTruck>(loadedState.Vehicles.First());

            Assert.Single(loadedState.Orders);
            Assert.IsType<PerishableCargo>(loadedState.Orders.First().Cargoes.First());
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}

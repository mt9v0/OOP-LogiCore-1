namespace LogiCore.App;

using System;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Events;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Logging;
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

public class DemoRunner
{
    public void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var validator = new CargoCompatibilityValidator();
        var deliveryService = new DeliveryService(validator);
        var reportsService = new ReportsService();
        var jsonStorage = new JsonStorageService();

        var consoleNotifier = new ConsoleNotifier();
        using var fileLogger = new FileLogger("logistics.log");


        deliveryService.OrderCreated += consoleNotifier.OnOrderCreated;
        deliveryService.OrderCreated += fileLogger.OnOrderCreated;

        deliveryService.OrderStatusChanged += consoleNotifier.OnOrderStatusChanged;
        deliveryService.OrderStatusChanged += fileLogger.OnOrderStatusChanged;

        deliveryService.VehicleOverloadAttempt += consoleNotifier.OnOverloadAttempt;
        deliveryService.VehicleOverloadAttempt += fileLogger.OnOverloadAttempt;

        deliveryService.DeliveryCompleted += consoleNotifier.OnDeliveryCompleted;
        deliveryService.DeliveryCompleted += fileLogger.OnDeliveryCompleted;

        var vehicleRepo = new Repository<Vehicle>();

        var vehicles = new List<Vehicle>
        {
            VehicleFactory.CreateVehicle("truck", "А069АА77", 5000, 30, 80, 50),
            VehicleFactory.CreateVehicle("refrigerator", "В777ВВ7", 4000, 25, 75, 65),
            VehicleFactory.CreateVehicle("plane", "RA-52000", 15000, 100, 700, 300),
            VehicleFactory.CreateVehicle("ship", "IMO-12345678", 50000, 500, 35, 20),
            VehicleFactory.CreateVehicle("drone", "DRONE-01", 15, 0.2, 50, 100),
            VehicleFactory.CreateVehicle("truck", "С067СС22", 10000, 50, 80, 55)
        };

        foreach (var v in vehicles)
        {
            vehicleRepo.Add(v);
            deliveryService.RegisterVehicle(v);
        }
        Console.WriteLine($"Зарегистрировано ТС в парке: {vehicles.Count}");

        var customer1 = new Customer("ООО ПА", "+7 812 111-22-33");
        var routeMsksPb = new Route("МСК - СПБ", new[] { new RoutePoint(55.7558, 37.6173), new RoutePoint(59.9342, 30.3351) });

        var invalidCargoes = new List<Cargo>
        {
            CargoFactory.CreateDangerous("Химикаты 3 класса", 500, 2, 50000, 3),
            CargoFactory.CreatePerishable("Замороженная рыба", 300, 1.5, 20000, DateTime.UtcNow.AddDays(5), -18)
        };

        try
        {
            deliveryService.CreateAndProcessOrder(customer1, routeMsksPb, invalidCargoes);
        }
        catch (IncompatibleCargoException)
        {
            Console.WriteLine("\n Несовместимые грузы!!");
        }

        var heavyTruck = vehicles.OfType<Truck>().First(t => t.MaxLoadKg == 5000);
        var heavyCargoes = new List<Cargo>
        {
            CargoFactory.CreateStandard("Оборудование 1", 3000, 10, 50000),
            CargoFactory.CreateStandard("Оборудование 2", 3000, 10, 50000)
        };

        try
        {
            validator.Validate(heavyTruck, heavyCargoes);
        }
        catch (VehicleOverloadException)
        {
            Console.WriteLine($"\nПерегрузик пу-пу-пу...");
        }
        
        var customer2 = new Customer("ИП Штанга", "+7 812 052-12-10");
        var validCargoes = new List<Cargo>
        {
            CargoFactory.CreateFragile("Серверное оборудование", 200, 1, 300000),
            CargoFactory.CreateStandard("Кабели и аксессуары", 150, 0.5, 50000)
        };

        var processedOrder = deliveryService.CreateAndProcessOrder(customer2, routeMsksPb, validCargoes);

        IDeliveryCost costCalculator = new BaseDeliveryCost(processedOrder.FinalCost);
        costCalculator = new InsuranceDecorator(costCalculator, validCargoes.Sum(c => c.DeclaredValue));
        costCalculator = new FragilePackingDecorator(costCalculator);
        costCalculator = new UrgentDeliveryDecorator(costCalculator);

        Console.WriteLine($"Детализация затрат: {costCalculator.Describe()}");
        Console.WriteLine($"Итоговая сумма со всеми услугами: {costCalculator.Total:C}");


        deliveryService.StartOrderDelivery(processedOrder);
        deliveryService.CompleteOrder(processedOrder);

        deliveryService.OrderCreated -= consoleNotifier.OnOrderCreated;

        foreach (var stat in reportsService.GetOrderStatsByStatus(deliveryService.Orders))
        {
            Console.WriteLine($"   Статус {stat.Status}: {stat.Count} шт., Сумма: {stat.TotalCost:C}");
        }

        Console.WriteLine("\nСериализация в JSON");
        string savePath = "system_state.json";
        var stateToSave = new SystemStateDto(
            deliveryService.Vehicles.ToList(),
            new List<Customer> { customer1, customer2 },
            deliveryService.Orders.ToList()
        );

        jsonStorage.SaveState(savePath, stateToSave);
        var loadedState = jsonStorage.LoadState(savePath);
        {
            Console.WriteLine($"Восстановлено заказов из файла: {loadedState.Orders.Count}");
        }
        RunInteractiveMenu(deliveryService);
        }

    private void RunInteractiveMenu(DeliveryService deliveryService)
    {
        while (true)
        {
            Console.WriteLine("\nГлавная страница");
            Console.WriteLine("1 Показать список зарегистрированного транспорта");
            Console.WriteLine("2 Показать историю всех заказов");
            Console.WriteLine("3 Показать выручку компании");
            Console.WriteLine("4 Выход");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            if (choice == "4") break;

            switch (choice)
            {
                case "1":
                    foreach (var v in deliveryService.Vehicles) Console.WriteLine($"   {v}");
                    break;
                case "2":
                    foreach (var o in deliveryService.Orders) Console.WriteLine($"   {o}");
                    break;
                case "3":
                    Console.WriteLine($"   Выручка: {deliveryService.TotalRevenue:C}");
                    break;
                default:
                    Console.WriteLine("   Неверный ввод.");
                    break;
            }
        }
    }
}
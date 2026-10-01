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
using LogiCore.Domain.Patterns.Singleton;
using LogiCore.Domain.Patterns.Strategy;

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

        Console.WriteLine("\nObserver: подписка наблюдателей на события DeliveryService ");
        Console.WriteLine("  Subject: DeliveryService (события OrderCreated, OrderStatusChanged, VehicleOverloadAttempt, DeliveryCompleted)");
        Console.WriteLine("  Observers: ConsoleNotifier, FileLogger\n");
        deliveryService.OrderCreated += consoleNotifier.OnOrderCreated;
        deliveryService.OrderCreated += fileLogger.OnOrderCreated;

        deliveryService.OrderStatusChanged += consoleNotifier.OnOrderStatusChanged;
        deliveryService.OrderStatusChanged += fileLogger.OnOrderStatusChanged;

        deliveryService.VehicleOverloadAttempt += consoleNotifier.OnOverloadAttempt;
        deliveryService.VehicleOverloadAttempt += fileLogger.OnOverloadAttempt;

        deliveryService.DeliveryCompleted += consoleNotifier.OnDeliveryCompleted;
        deliveryService.DeliveryCompleted += fileLogger.OnDeliveryCompleted;

        deliveryService.SystemLogged += consoleNotifier.OnSystemLog;

        Console.WriteLine("Наблюдатели подписаны (4 события × 2 подписчика = 8 подписок)\n");

        var vehicleRepo = new Repository<Vehicle>();
        Console.WriteLine("\nFactory Method: создание транспорта через VehicleFactory");
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
        
        // singleton
        Console.WriteLine("\nSingleton: глобальная конфигурация тарифов через Lazy<T>");
        var config = TariffConfig.Instance;
        Console.WriteLine($"TariffConfig.Instance: скидка {config.BaseDiscountRate:P0}, страховка {config.DefaultInsuranceRate:P0}");

        var customer1 = new Customer("ООО ПА", "+7 812 111-22-33");
        var routeMsksPb = new Route("МСК - СПБ", new[] { new RoutePoint(55.7558, 37.6173), new RoutePoint(59.9342, 30.3351) });
        
        Console.WriteLine("\nFactory Method: создание грузов через CargoFactory");
        var cargoPool = new List<Cargo>
        {
            CargoFactory.CreateStandard("Коробки с одеждой", 100, 1, 5000),
            CargoFactory.CreateStandard("Мебель", 500, 5, 50000),
            CargoFactory.CreateStandard("Бытовая техника", 300, 3, 80000),
            CargoFactory.CreatePerishable("Молоко", 50, 0.5, 1000, DateTime.UtcNow.AddDays(2), 4),
            CargoFactory.CreatePerishable("Замороженная рыба", 300, 1.5, 20000, DateTime.UtcNow.AddDays(5), -18),
            CargoFactory.CreateFragile("Серверное оборудование", 200, 1, 300000),
            CargoFactory.CreateFragile("Стеклянная посуда", 80, 0.5, 40000),
            CargoFactory.CreateDangerous("Химикаты 3 класса", 500, 2, 50000, 3),
            CargoFactory.CreateDangerous("Газовый баллон", 100, 0.5, 30000, 2),
            CargoFactory.CreateOversized("Промышленный станок", 8000, 20, 500000, 2.5),
            CargoFactory.CreateOversized("Турбина", 12000, 30, 800000, 4.0)
        };
        Console.WriteLine($"Создано грузов: {cargoPool.Count}");

        try
        {
            deliveryService.CreateAndProcessOrder(customer1, routeMsksPb, new List<Cargo>
            {
                cargoPool[7], // опасный
                cargoPool[4] // скоропортящийся
            });
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
        catch (VehicleOverloadException ex)
        {
            Console.WriteLine($"\nПерегрузик пу-пу-пу... {ex.Message}");
            deliveryService.ReportOverloadAttempt(heavyTruck, ex.AttemptedWeight);
        }
        
        var customer2 = new Customer("ИП Штанга", "+7 812 052-12-10");
        var customer3 = new Customer("ООО Ромашка", "+7 495 123-45-67");

        var routeMsksKazan = new Route("МСК - Казань", new[]
        {
            new RoutePoint(55.7558, 37.6173),
            new RoutePoint(55.7963, 49.1088)
        });

        var orderSpecs = new (Customer Customer, Route Route, int[] CargoIndexes)[]
        {
            (customer2, routeMsksPb, new[] { 5, 0 }),      // fragile + standard
            (customer3, routeMsksKazan, new[] { 1 }),      // standard
            (customer1, routeMsksPb, new[] { 3 }),         // perishable
            (customer2, routeMsksPb, new[] { 8 }),         // dangerous
            (customer3, routeMsksKazan, new[] { 9 })       // oversized
        };

        var processedOrders = new List<Order>();
        
        Console.WriteLine("\nDecorator: цепочка услуг поверх базовой стоимости");
        foreach (var spec in orderSpecs)
        {
            var cargoes = spec.CargoIndexes.Select(i => cargoPool[i]).ToList();
            try
            {
                var order = deliveryService.CreateAndProcessOrder(spec.Customer, spec.Route, cargoes);

                // decorator
                IDeliveryCost cost = new BaseDeliveryCost(order.FinalCost);
                Console.WriteLine($"  Слой 1 (базовая):     {cost.Total:C}");

                cost = new InsuranceDecorator(cost, cargoes.Sum(c => c.DeclaredValue));
                Console.WriteLine($"  Слой 2 (+страховка):  {cost.Total:C}");

                cost = new FragilePackingDecorator(cost);
                Console.WriteLine($"  Слой 3 (+упаковка):   {cost.Total:C}");

                cost = new UrgentDeliveryDecorator(cost);
                Console.WriteLine($"  Слой 4 (+срочность):  {cost.Total:C}");

                Console.WriteLine($"\nЗаказ {order.Id.ToString()[..8]} ({spec.Customer.Name})");
                Console.WriteLine($"  {cost.Describe()}");
                Console.WriteLine($"  Итог со всеми услугами: {cost.Total:C}");

                order.UpdateFinalCost(cost.Total);

                processedOrders.Add(order);
            }
            catch (RouteNotFoundException ex)
            {
                Console.WriteLine($"Не удалось подобрать ТС: {ex.Message}");
            }
        }

        // strategy
        if (processedOrders.Count > 0)
        {
            var sampleOrder = processedOrders[0];
            var strategies = new ITariffStrategy[]
            {
                new StandardTariff(),
                new ExpressTariff(),
                new HeavyCargoTariff()
            };

            Console.WriteLine($"\nStrategy: Применение тарифных стратегий к заказу {sampleOrder.Id.ToString()[..8]}");
            foreach (var strategy in strategies)
            {
                var cost = strategy.Calculate(sampleOrder.FinalCost, sampleOrder.Route, sampleOrder.Cargoes);
                Console.WriteLine($"  {strategy.Name}: {cost:C}");
            }
        }

        foreach (var order in processedOrders.Take(3))
        {
            deliveryService.StartOrderDelivery(order);
            deliveryService.CompleteOrder(order);
        }

        Console.WriteLine("\nObserver: демонстрация отписки");
        Console.WriteLine("Отписываем ConsoleNotifier от события OrderCreated (consoleNotifier больше не получит уведомления о новых заказах)");
        deliveryService.OrderCreated -= consoleNotifier.OnOrderCreated;
        deliveryService.SystemLogged -= consoleNotifier.OnSystemLog;

        Console.WriteLine("\nLINQ-отчеты");
        reportsService.PrintAllReports(deliveryService, Console.Out);

        Console.WriteLine("\nСериализация в JSON");
        string savePath = "system_state.json";
        var stateToSave = new SystemStateDto(
            deliveryService.Vehicles.ToList(),
            new List<Customer> { customer1, customer2 },
            deliveryService.Orders.ToList()
        );

        decimal revenueBefore = deliveryService.TotalRevenue;
        int ordersBefore = deliveryService.Orders.Count;

        jsonStorage.SaveState(savePath, stateToSave);
        Console.WriteLine($"Состояние сохранено: заказов — {ordersBefore}, выручка — {revenueBefore:C}");

        var loadedState = jsonStorage.LoadState(savePath);
        Console.WriteLine($"Загружено из файла: заказов — {loadedState.Orders.Count}, ТС — {loadedState.Vehicles.Count}");

        bool match = loadedState.Orders.Count == ordersBefore
                && loadedState.Vehicles.Count == vehicles.Count;
        Console.WriteLine(match
            ? "Данные совпадают с состоянием до сохранения"
            : "Данные НЕ совпадают — что-то потерялось!");
        
        RunInteractiveMenu(deliveryService, reportsService, jsonStorage,
            new[] { customer1, customer2, customer3 });
    }

    private void RunInteractiveMenu(
        DeliveryService deliveryService,
        ReportsService reportsService,
        JsonStorageService jsonStorage,
        IReadOnlyCollection<Customer> customers)
    {
        while (true)
        {
            Console.WriteLine("\n===== Главное меню =====");
            Console.WriteLine("1. Показать список транспорта");
            Console.WriteLine("2. Показать историю заказов");
            Console.WriteLine("3. Показать выручку компании");
            Console.WriteLine("4. Показать все отчёты");
            Console.WriteLine("5. Сохранить состояние");
            Console.WriteLine("6. Загрузить состояние");
            Console.WriteLine("7. Выход");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    foreach (var v in deliveryService.Vehicles)
                        Console.WriteLine($"   {v}");
                    break;
                case "2":
                    foreach (var o in deliveryService.Orders)
                        Console.WriteLine($"   {o}");
                    break;
                case "3":
                    Console.WriteLine($"   Выручка: {deliveryService.TotalRevenue:C}");
                    break;
                case "4":
                    reportsService.PrintAllReports(deliveryService, Console.Out);
                    break;
                case "5":
                    jsonStorage.SaveState("system_state.json", new SystemStateDto(
                        deliveryService.Vehicles.ToList(),
                        customers.ToList(),
                        deliveryService.Orders.ToList()));
                    Console.WriteLine("   Состояние сохранено в system_state.json");
                    break;
                case "6":
                    try
                    {
                        var loaded = jsonStorage.LoadState("system_state.json");
                        Console.WriteLine($"   Загружено: заказов — {loaded.Orders.Count}, ТС — {loaded.Vehicles.Count}");
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("   Файл system_state.json не найден.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"   Ошибка загрузки: {ex.Message}");
                    }
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("   Неверный ввод.");
                    break;
            }
        }
    }
}
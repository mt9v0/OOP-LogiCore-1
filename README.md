# LogiCore — ядро информационной системы логистики

Консольное приложение на C# (.NET 9), моделирующее работу транспортно-логистической компании: приём заказов, проверку совместимости грузов, подбор транспорта, расчёт стоимости, отчётность и сохранение состояния.

## Структура решения

Решение состоит из 3 проектов:

- LogiCore.Domain — доменная модель и бизнес-логика. Не зависит от консоли.
- LogiCoreApp — консольное приложение с автодемонстрацией и меню.
- LogiCore.Tests — юнит-тесты на xUnit.

## Архитектура

Домен разбит по слоям:

- Models/Cargoes/ — иерархия грузов: Cargo и 5 наследников в отдельных файлах.
- Models/Vehicles/ — иерархия транспорта: Vehicle и 5 наследников в отдельных файлах.
- Models/Orders/ — заказ и его жизненный цикл.
- Models/Customers/ — клиент.
- Models/Route.cs, Models/RoutePoint.cs — маршрут и точка.
- Enums/ — перечисления VehicleState, OrderStatus, TransportConditions.
- Interfaces/ — контракты домена.
- Repositories/ — Repository<T> с итератором на yield return.
- Services/ — DeliveryService, CargoCompatibilityValidator, ReportsService, JsonStorageService.
- Events/ + Logging/ — события и подписчики (Observer на событиях C#).
- Patterns/ — 5 паттернов: Strategy, Decorator, Factory Method, Observer, Singleton.
- Exceptions/ — иерархия от LogisticsException.
- Extensions/ — метод расширения ToReportTable().

LogiCoreApp при запуске прогоняет автодемонстрацию (парк ТС, грузы, 5 заказов, отчёты, JSON), затем показывает интерактивное меню.

## Таблица соответствия Т1–T10 + E3

| Требование | Описание | Класс / Файл |
| :--- | :--- | :--- |
| T1. Инкапсуляция | Приватные сеттеры, валидация инвариантов в конструкторах, коллекции наружу как IReadOnlyCollection | Models/Cargoes/Cargo.cs, Models/Vehicles/Vehicle.cs, Models/Orders/Order.cs, Models/Customers/Customers.cs, Models/Route.cs |
| T2. Наследование и полиморфизм | Иерархии Vehicle и Cargo, sealed-класс DroneCourier, вызовы base.CanCarry() | Models/Vehicles/Vehicle.cs, Models/Vehicles/DroneCourier.cs, Models/Vehicles/RefrigeratorTruck.cs, Models/Cargoes/Cargo.cs |
| T3. Интерфейсы и вариантность | 4+ интерфейса, явная реализация IStackable, ковариантный IReadOnlyRepository<out T>, контравариантный IValidator<in T> | Interfaces/IStackable.cs, Interfaces/IReadOnlyRepository.cs, Interfaces/IValidator.cs, Models/Cargoes/StandardCargo.cs |
| T4. Обобщения и коллекции | Repository<T> с yield return, метод расширения ToReportTable() | Repositories/Repository.cs, Extensions/ReportExtensions.cs |
| T5. Делегаты и события | 4 класса EventArgs, собственный делегат SystemLogHandler, независимые подписчики ConsoleNotifier и FileLogger, отписка через -= | Events/Delegates.cs, Events/*EventArgs.cs, Logging/ConsoleNotifier.cs, Logging/FileLogger.cs |
| T6. Исключения | Базовый LogisticsException и 5 наследников, обработка с when, using/IDisposable для файловых ресурсов | Exceptions/LogisticsException/LogisticsException.cs, Services/CargoCompatibilityValidator.cs, Logging/FileLogger.cs |
| T7. Паттерны (×5) | Strategy (тарифы), Decorator (услуги), Factory Method (создание транспорта и грузов), Observer (события), Singleton (TariffConfig через Lazy<T>) | Patterns/Strategy/, Patterns/Decorator/, Patterns/Factory/, Patterns/Observer/, Patterns/Singleton/ |
| T8. LINQ-отчёты | 6 отчётов: агрегаты, группировки, Join, синтаксис from...select, ToDictionary | Services/ReportsService.cs |
| T9 / E3. Сериализация | Полиморфная JSON-сериализация через [JsonDerivedType], восстановление точных типов | Services/JsonStorageService.cs, Models/Cargoes/Cargo.cs, Models/Vehicles/Vehicle.cs |
| T10. Enum и структуры | [Flags]-перечисление TransportConditions, структура RoutePoint с перегрузкой - и явным приведением к string | Enums/TransportConditions.cs, Models/RoutePoint.cs |

## SOLID в проекте
- SRP — правила совместимости грузов вынесены из моделей в отдельный CargoCompatibilityValidator.
- OCP — новый транспорт добавляется наследником Vehicle без правок существующего кода; новая услуга — декоратором IDeliveryCost.
- LSP — любой наследник Vehicle корректно подставляется в DeliveryService.FindBestVehicle и работает через CanCarry / CalculateDeliveryCost.
- ISP — узкие интерфейсы ITemperatureSensitive, IInsurable, IStackable вместо одного «толстого».
- DIP — DeliveryService работает с абстрактным Vehicle и валидатором CargoCompatibilityValidator, а не с конкретными типами транспорта.

## Запуск

Сборка:

    dotnet build

Тесты:

    dotnet test

Демо + интерактивное меню:

    dotnet run --project LogiCoreApp

При запуске приложение само прогоняет демо-сценарий (создание парка, грузов, заказов, отчёты, JSON), затем открывает меню.

## Паттерны в демонстрации

В выводе демо каждый паттерн помечен заголовком:

- === Observer: ... === — подписка ConsoleNotifier и FileLogger на 4 события DeliveryService, отписка через -=.
- === Factory Method: ... === — VehicleFactory.CreateVehicle(...) и CargoFactory.CreateXxx(...).
- === Singleton: ... === — TariffConfig.Instance (Lazy).
- === Decorator: ... === — пошаговое наращивание стоимости: BaseDeliveryCost → Insurance → FragilePacking → UrgentDelivery.
- === Strategy: ... === — применение трёх тарифных стратегий к одному заказу.

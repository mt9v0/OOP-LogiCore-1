LogiCore — Ядро иформационной системы логистики

Решение состоит из 3 проектов:
1. **LogiCore.Domain** — доменная модель и бизнес-логика
2. **LogiCore.App** — автодемонстрация и консольное меню
3. **LogiCore.Tests** — юнит-тесты

Таблица соответствия технических требований (1–10 + E3)

| Требование | Описание | Класс / Файл |
| :--- | :--- | :--- |
| **T1. Инкапсуляция** | Приватные сеттеры, валидация инвариантов в конструкторах, коллекция `IReadOnlyCollection` | `Cargo.cs`, `Vehicle.cs`, `Order.cs` |
| **T2. Наследование и Полиморфизм** | Иерархии `Vehicle` и `Cargo`, `sealed`-класс `DroneCourier`, вызовы `base.CanCarry()` | `Vehicle.cs`, `DroneCourier.cs`, `RefrigeratorTruck.cs` |
| **T3. Интерфейсы и Вариантность** | 4 интерфейса, явная реализация `IStackable`, ковариантность `IReadOnlyRepository<out T>`, контравариантность `IValidator<in T>` | `IStackable.cs`, `StandardCargo.cs`, `IReadOnlyRepository.cs`, `IValidator.cs` |
| **T4. Обобщения и Коллекции** | `Repository<T>`, собственная итерация через `yield return`, метод расширения `ToReportTable()` | `Repository.cs`, `ReportExtensions.cs` |
| **T5. Делегаты и События** | 4 класса `EventArgs`, собственный `delegate`, независимые подписчики `ConsoleNotifier` и `FileLogger` | `Delegates.cs`, `ConsoleNotifier.cs`, `FileLogger.cs` |
| **T6. Исключения** | Базовый `LogisticsException` и 5 наследников, фильтры `when`, использование `using`/`IDisposable` | `LogisticsException.cs`, `CargoCompatibilityValidator.cs`, `FileLogger.cs` |
| **T7. Паттерны (×5)** | **Strategy** (тарифы), **Decorator** (услуги), **Factory Method** (создание объектов), **Observer** (события), **Singleton** (`TariffConfig` через `Lazy<T>`) | Папка `Patterns/` (`Strategy`, `Decorator`, `Factory`, `Observer`, `Singleton`) |
| **T8. LINQ-отчёты** | 6 отчётов (в т.ч. в синтаксисе запросов `from...select`), агрегаты, группировки, Join | `ReportsService.cs` |
| **T9 / E3. Сериализация** | Полная полиморфная JSON-сериализация с восстановлением точных типов через `[JsonDerivedType]` | `JsonStorageService.cs`, `Cargo.cs`, `Vehicle.cs` |
| **T10. Enum и Структуры** | `[Flags]`-перечисление `TransportConditions`, структура `RoutePoint` с перегрузкой оператора `-` и явным приведением | `TransportConditions.cs`, `RoutePoint.cs` |

---

Принципы SOLID в проекте
1. **SRP:** Бизнес-правила валидации вынесены из классов моделей в `CargoCompatibilityValidator`
2. **OCP:** Новые типы транспорта и грузов добавляются расширением классов без изменения существующих
3. **LSP:** Любой наследник `Vehicle` корректно подставляется в алгоритм `DeliveryService.FindBestVehicle`
4. **ISP:** Узкие интерфейсы `ITemperatureSensitive`, `IInsurable`, `IStackable`
5. **DIP:** `DeliveryService` зависит от абстракций `Vehicle`
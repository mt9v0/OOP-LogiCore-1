namespace LogiCore.Domain.Patterns.Observer;

// Паттерн "Наблюдатель" реализован через события C#.
// Издатель — DeliveryService, у него 4 события:
//   OrderCreated, OrderStatusChanged, VehicleOverloadAttempt, DeliveryCompleted.
// Подписчики — ConsoleNotifier (пишет в консоль) и FileLogger (пишет в файл).
// Подписка/отписка — через += и -= (см. DemoRunner).
public static class ObserverNotes { }
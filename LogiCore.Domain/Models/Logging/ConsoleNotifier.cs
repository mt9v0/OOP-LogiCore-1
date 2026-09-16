namespace LogiCore.Domain.Logging;

using LogiCore.Domain.Events;

public class ConsoleNotifier
{
    public void OnOrderCreated(object? sender, OrderCreatedEventArgs e) =>
        Console.WriteLine($"Создан заказ №{e.Order.Id.ToString()[..8]} для клиента {e.Order.Customer.Name}");

    public void OnOrderStatusChanged(object? sender, OrderStatusChangedEventArgs e) =>
        Console.WriteLine($"Заказ №{e.Order.Id.ToString()[..8]}: статус изменен {e.OldStatus} -> {e.NewStatus}");

    public void OnOverloadAttempt(object? sender, VehicleOverloadAttemptEventArgs e) =>
        Console.WriteLine($"Внимание! Попытка перегруза ТС {e.Vehicle.RegistrationNumber}! Запрошено: {e.AttemptedWeight}кг");

    public void OnDeliveryCompleted(object? sender, DeliveryCompletedEventArgs e) =>
        Console.WriteLine($"Доставка заказа №{e.Order.Id.ToString()[..8]} успешно завершена!");
}
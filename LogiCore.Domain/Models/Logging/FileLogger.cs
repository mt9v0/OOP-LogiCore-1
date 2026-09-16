namespace LogiCore.Domain.Logging;

using LogiCore.Domain.Events;

public class FileLogger : IDisposable
{
    private readonly StreamWriter _writer;

    public FileLogger(string logFilePath)
    {
        // Использование файлового потока (Т5, Т6)
        _writer = new StreamWriter(logFilePath, append: true) { AutoFlush = true };
    }

    public void OnOrderCreated(object? sender, OrderCreatedEventArgs e) =>
        Log($"Заказ создан {e.Order.Id} для Заказчика {e.Order.Customer.Name}");

    public void OnOrderStatusChanged(object? sender, OrderStatusChangedEventArgs e) =>
        Log($"Статус заказ {e.Order.Id}: {e.OldStatus} -> {e.NewStatus}");

    public void OnOverloadAttempt(object? sender, VehicleOverloadAttemptEventArgs e) =>
        Log($" Перегруз транспорта {e.Vehicle.RegistrationNumber}, Вес: {e.AttemptedWeight}kg");

    public void OnDeliveryCompleted(object? sender, DeliveryCompletedEventArgs e) =>
        Log($"Заказ {e.Order.Id} выполнен в {e.CompletedAt}");

    private void Log(string message)
    {
        _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
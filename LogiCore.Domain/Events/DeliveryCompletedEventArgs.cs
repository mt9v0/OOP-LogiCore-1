namespace LogiCore.Domain.Events;

using LogiCore.Domain.Models.Orders;

public class DeliveryCompletedEventArgs : EventArgs
{
    public Order Order { get; }
    public DateTime CompletedAt { get; }

    public DeliveryCompletedEventArgs(Order order)
    {
        Order = order;
        CompletedAt = DateTime.UtcNow;
    }
}
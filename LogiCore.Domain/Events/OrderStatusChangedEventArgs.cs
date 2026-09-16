namespace LogiCore.Domain.Events;

using LogiCore.Domain.Enums;
using LogiCore.Domain.Models.Orders;

public class OrderStatusChangedEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEventArgs(Order order, OrderStatus oldStatus, OrderStatus newStatus)
    {
        Order = order;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
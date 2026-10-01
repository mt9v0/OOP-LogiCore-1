namespace LogiCore.Domain.Events;

using LogiCore.Domain.Models.Orders;

public class OrderCreatedEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderCreatedEventArgs(Order order) => Order = order;
}
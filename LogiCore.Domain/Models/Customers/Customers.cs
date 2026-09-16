namespace LogiCore.Domain.Models.Customers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LogiCore.Domain.Interfaces;
using LogiCore.Domain.Models.Orders;

public class Customer : IEntity
{
    private readonly List<Order> _orders = new();

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;

    [JsonIgnore]
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public Customer() { }

    public Customer(string name, string contactInfo)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя клиента не может быть пустым...", nameof(name));
        if (string.IsNullOrWhiteSpace(contactInfo))
            throw new ArgumentException("Контактные данные не могут быть пустыми...", nameof(contactInfo));

        Id = Guid.NewGuid();
        Name = name.Trim();
        ContactInfo = contactInfo.Trim();
    }

    internal void AddOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!_orders.Any(o => o.Id == order.Id))
        {
            _orders.Add(order);
        }
    }
}
namespace LogiCore.Domain.Services;

using LogiCore.Domain.Enums;
using LogiCore.Domain.Models.Cargoes;
using LogiCore.Domain.Models.Customers;
using LogiCore.Domain.Models.Orders;
using LogiCore.Domain.Models.Vehicles;

public class ReportsService
{

    public IEnumerable<(Vehicle Vehicle, decimal Revenue)> GetTop3VehiclesByRevenue(IEnumerable<Order> orders)
    {
        return orders
            .Where(o => o.Status == OrderStatus.Delivered && o.AssignedVehicle != null)
            .GroupBy(o => o.AssignedVehicle!)
            .Select(g => (Vehicle: g.Key, Revenue: g.Sum(o => o.FinalCost)))
            .OrderByDescending(x => x.Revenue)
            .Take(3);
    }

    public IEnumerable<(OrderStatus Status, int Count, decimal TotalCost)> GetOrderStatsByStatus(IEnumerable<Order> orders)
    {
        return orders
            .GroupBy(o => o.Status)
            .Select(g => (Status: g.Key, Count: g.Count(), TotalCost: g.Sum(o => o.FinalCost)));
    }

    public IEnumerable<(string VehicleType, double AvgLoadPercentage)> GetAverageLoadByVehicleType(IEnumerable<Order> orders)
    {
        return orders
            .Where(o => o.AssignedVehicle != null)
            .GroupBy(o => o.AssignedVehicle!.GetType().Name)
            .Select(g => (
                VehicleType: g.Key,
                AvgLoadPercentage: g.Average(o => (o.Cargoes.Sum(c => c.WeightKg) / o.AssignedVehicle!.MaxLoadKg) * 100.0)
            ));
    }

    public IEnumerable<(Customer Customer, decimal TotalSpent)> GetVIPCustomers(IEnumerable<Customer> customers, decimal threshold)
    {
        return customers
            .Select(c => (Customer: c, TotalSpent: c.Orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.FinalCost)))
            .Where(x => x.TotalSpent >= threshold)
            .OrderByDescending(x => x.TotalSpent);
    }

    public IEnumerable<string> GetCargoCustomerMapQuerySyntax(IEnumerable<Order> orders)
    {
        var result = from order in orders
                     from cargo in order.Cargoes
                     select $"Груз: '{cargo.Description}' ({cargo.WeightKg} кг) | Заказ №{order.Id.ToString()[..8]} | Клиент: {order.Customer.Name}";

        return result;
    }

    public Dictionary<int, int> GetDangerousCargoCountByClass(IEnumerable<Order> orders)
    {
        return orders
            .SelectMany(o => o.Cargoes)
            .OfType<DangerousCargo>()
            .GroupBy(c => c.DangerClass)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
namespace LogiCore.Domain.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogiCore.Domain.Models.Customers;
using LogiCore.Domain.Models.Orders;
using LogiCore.Domain.Models.Vehicles;

public record SystemStateDto(
    List<Vehicle> Vehicles,
    List<Customer> Customers,
    List<Order> Orders
);

public class JsonStorageService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles // Игнорирование зацикленных ссылок (Customer <-> Order)
    };

    public void SaveState(string filePath, SystemStateDto state)
    {
        string json = JsonSerializer.Serialize(state, _options);
        File.WriteAllText(filePath, json);
    }

    public SystemStateDto LoadState(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл состояния не найден: {filePath}");

        try
        {
            string json = File.ReadAllText(filePath);
            var state = JsonSerializer.Deserialize<SystemStateDto>(json, _options);
            return state ?? throw new InvalidOperationException("JSON пустой или имеет неправильный формат");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Файл состояния повреждён: {filePath}", ex);
        }
    }
}
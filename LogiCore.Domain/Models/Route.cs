namespace LogiCore.Domain.Models.Routes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LogiCore.Domain.Models;

public class Route
{
    public string Name { get; set; } = string.Empty;
    public List<RoutePoint> Points { get; set; } = new();

    [JsonIgnore]
    public double DistanceKm
    {
        get
        {
            if (Points == null || Points.Count < 2) return 0;
            
            double totalDistance = 0;
            for (int i = 0; i < Points.Count - 1; i++)
            {
                totalDistance += Points[i + 1] - Points[i];
            }
            return totalDistance;
        }
    }

    public Route() { }

    public Route(string name, IEnumerable<RoutePoint> points)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        var pointList = points?.ToList() ?? throw new ArgumentNullException(nameof(points));
        
        if (pointList.Count < 2)
            throw new ArgumentException("Маршрут должен содержать минимум 2 точки", nameof(points));

        Name = name;
        Points = pointList;
    }

    public TimeSpan EstimateTime(double speedKmH)
    {
        if (speedKmH <= 0) return TimeSpan.Zero;
        double hours = DistanceKm / speedKmH;
        return TimeSpan.FromHours(hours);
    }
}
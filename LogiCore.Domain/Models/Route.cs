namespace LogiCore.Domain.Models.Routes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LogiCore.Domain.Models;

public class Route
{
    private readonly List<RoutePoint> _points = new();

    [JsonInclude]
    public string Name { get; private set; } = string.Empty;

    [JsonInclude]
    public IReadOnlyCollection<RoutePoint> Points => _points;

    [JsonIgnore]
    public double DistanceKm
    {
        get
        {
            if (_points.Count < 2) return 0;
            double totalDistance = 0;
            for (int i = 0; i < _points.Count - 1; i++)
                totalDistance += _points[i + 1] - _points[i];
            return totalDistance;
        }
    }

    [JsonConstructor]
    public Route(string name, IReadOnlyCollection<RoutePoint> points)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        var pointList = points?.ToList() ?? throw new ArgumentNullException(nameof(points));
        if (pointList.Count < 2)
            throw new ArgumentException("Маршрут должен содержать минимум 2 точки", nameof(points));
        _points = pointList;
    }

    public TimeSpan EstimateTime(double speedKmH)
    {
        if (speedKmH <= 0) return TimeSpan.Zero;
        return TimeSpan.FromHours(DistanceKm / speedKmH);
    }
}
namespace LogiCore.Domain.Models;

/// <summary>
/// Географическая точка маршрута — структура (struct) с перегрузкой оператора "-" и явным приведением (T10 [1, 5]).
/// </summary>
public readonly struct RoutePoint
{
    public double Latitude { get; }
    public double Longitude { get; }

    public RoutePoint(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    // Перегрузка оператора "-" для расчета расстояния между двумя точками (T10 [5])
    public static double operator -(RoutePoint a, RoutePoint b)
    {
        double dx = a.Latitude - b.Latitude;
        double dy = a.Longitude - b.Longitude;
        return Math.Sqrt(dx * dx + dy * dy) * 111.0;
    }

    // Явное приведение типа в string (T10 [5])
    public static explicit operator string(RoutePoint point)
    {
        return $"[{point.Latitude:F4}, {point.Longitude:F4}]";
    }

    public override string ToString() => (string)this;
}
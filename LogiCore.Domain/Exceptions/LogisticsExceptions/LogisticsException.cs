namespace LogiCore.Domain.Exceptions;

using System;

public class LogisticsException : Exception
{
    public LogisticsException(string message) : base(message) { }
    public LogisticsException(string message, Exception innerException) : base(message, innerException) { }
}

public class CargoValidationException : LogisticsException
{
    public CargoValidationException(string message) : base(message) { }
}

public class IncompatibleCargoException : LogisticsException
{
    public IncompatibleCargoException(string message) : base(message) { }
}

public class VehicleOverloadException : LogisticsException
{
    public double AttemptedWeight { get; }
    public double MaxWeight { get; }

    public VehicleOverloadException(string message, double attemptedWeight = 0, double maxWeight = 0) 
        : base(message)
    {
        AttemptedWeight = attemptedWeight;
        MaxWeight = maxWeight;
    }
}

public class RouteNotFoundException : LogisticsException
{
    public RouteNotFoundException(string message) : base(message) { }
}

public class InvalidOrderStateException : LogisticsException
{
    public InvalidOrderStateException(string message) : base(message) { }
}
namespace LogiCore.Domain.Events;

using LogiCore.Domain.Models.Vehicles;

public class VehicleOverloadAttemptEventArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public double AttemptedWeight { get; }

    public VehicleOverloadAttemptEventArgs(Vehicle vehicle, double attemptedWeight)
    {
        Vehicle = vehicle;
        AttemptedWeight = attemptedWeight;
    }
}
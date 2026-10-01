namespace LogiCore.Domain.Interfaces;

public interface IValidator<in T>
{
    bool Validate(T item, out string errorMessage);
}
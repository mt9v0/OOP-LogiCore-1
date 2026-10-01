namespace LogiCore.Domain.Interfaces;

public interface IReadOnlyRepository<out T> where T : class, IEntity
{
    T? GetById(Guid id);
    IEnumerable<T> GetAll();
}
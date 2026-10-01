namespace LogiCore.Domain.Repositories;

using System.Collections;
using LogiCore.Domain.Interfaces;

public class Repository<T> : IReadOnlyRepository<T>, IEnumerable<T> where T : class, IEntity
{
    private readonly Dictionary<Guid, T> _items = new();

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items[item.Id] = item;
    }

    public bool Remove(Guid id) => _items.Remove(id);

    public T? GetById(Guid id) => _items.TryGetValue(id, out var item) ? item : null;

    public IEnumerable<T> GetAll() => this;

    public IEnumerable<T> FindAll(Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        foreach (var item in _items.Values)
        {
            if (match(item))
                yield return item;
        }
    }

    public T? this[Guid id] => GetById(id);

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items.Values)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}

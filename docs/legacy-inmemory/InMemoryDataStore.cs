using System.Collections.Concurrent;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Infrastructure;

public class InMemoryDataStore
{
    private readonly ConcurrentDictionary<Type, object> _sets = new();
    private readonly ConcurrentDictionary<Type, int> _sequences = new();
    public object SyncRoot { get; } = new();

    public List<T> Set<T>() where T : BaseEntity
        => (List<T>)_sets.GetOrAdd(typeof(T), _ => new List<T>());

    public int NextId<T>() where T : BaseEntity
        => _sequences.AddOrUpdate(typeof(T), 1, (_, current) => current + 1);

    public void SetSequence<T>(int value) where T : BaseEntity
        => _sequences[typeof(T)] = value;
}

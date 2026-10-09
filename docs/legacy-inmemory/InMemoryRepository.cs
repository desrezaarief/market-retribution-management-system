using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Infrastructure.Repositories;

public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly InMemoryDataStore _store;
    private readonly IHttpContextAccessor _http;

    public InMemoryRepository(InMemoryDataStore store, IHttpContextAccessor http)
    {
        _store = store;
        _http = http;
    }

    private string Actor => _http.HttpContext?.User?.Identity?.Name ?? "system";

    public Task<IReadOnlyList<T>> GetAllAsync()
    {
        lock (_store.SyncRoot)
            return Task.FromResult<IReadOnlyList<T>>(_store.Set<T>().ToList());
    }

    public Task<T?> GetByIdAsync(int id)
    {
        lock (_store.SyncRoot)
            return Task.FromResult(_store.Set<T>().FirstOrDefault(x => x.Id == id));
    }

    public Task<T> AddAsync(T entity)
    {
        lock (_store.SyncRoot)
        {
            entity.Id = _store.NextId<T>();
            entity.CreatedAt = DateTime.Now;
            entity.CreatedBy = Actor;
            _store.Set<T>().Add(entity);
            return Task.FromResult(entity);
        }
    }

    public Task UpdateAsync(T entity)
    {
        lock (_store.SyncRoot)
        {
            var list = _store.Set<T>();
            var index = list.FindIndex(x => x.Id == entity.Id);
            if (index < 0) throw new KeyNotFoundException($"{typeof(T).Name} #{entity.Id} tidak ditemukan.");
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedBy = Actor;
            entity.CreatedAt = list[index].CreatedAt;
            entity.CreatedBy = list[index].CreatedBy;
            list[index] = entity;
            return Task.CompletedTask;
        }
    }

    public Task DeleteAsync(int id)
    {
        lock (_store.SyncRoot)
        {
            var list = _store.Set<T>();
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null) list.Remove(item);
            return Task.CompletedTask;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> Set;
    private readonly IHttpContextAccessor _http;

    public EfRepository(AppDbContext db, IHttpContextAccessor http)
    {
        Db = db;
        Set = db.Set<T>();
        _http = http;
    }

    protected string Actor => _http.HttpContext?.User?.Identity?.Name ?? "system";

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        => await Set.AsNoTracking().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(int id)
        => await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.Id = 0;
        entity.CreatedAt = DateTime.Now;
        entity.CreatedBy = Actor;
        entity.UpdatedAt = null;
        entity.UpdatedBy = null;

        await Set.AddAsync(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        var existing = await Set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.Id)
            ?? throw new KeyNotFoundException($"{typeof(T).Name} #{entity.Id} tidak ditemukan.");

        entity.CreatedAt = existing.CreatedAt;
        entity.CreatedBy = existing.CreatedBy;
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = Actor;

        Set.Update(entity);
        await Db.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await Set.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return;

        Set.Remove(entity);
        await Db.SaveChangesAsync();
    }
}

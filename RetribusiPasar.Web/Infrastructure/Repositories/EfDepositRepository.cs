using Microsoft.EntityFrameworkCore;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Infrastructure.Repositories;

public class EfDepositRepository : IRepository<Deposit>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public EfDepositRepository(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    private string Actor => _http.HttpContext?.User?.Identity?.Name ?? "system";

    public async Task<IReadOnlyList<Deposit>> GetAllAsync()
    {
        var deposits = await _db.Deposits.AsNoTracking().ToListAsync();
        var links = await _db.DepositReceipts.AsNoTracking().ToListAsync();
        var byDeposit = links.GroupBy(x => x.DepositId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ReceiptId).ToList());

        foreach (var deposit in deposits)
            deposit.ReceiptIds = byDeposit.GetValueOrDefault(deposit.Id, new List<int>());

        return deposits;
    }

    public async Task<Deposit?> GetByIdAsync(int id)
    {
        var deposit = await _db.Deposits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (deposit == null) return null;

        deposit.ReceiptIds = await _db.DepositReceipts.AsNoTracking()
            .Where(x => x.DepositId == id)
            .Select(x => x.ReceiptId)
            .ToListAsync();

        return deposit;
    }

    public async Task<Deposit> AddAsync(Deposit entity)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        entity.Id = 0;
        entity.CreatedAt = DateTime.Now;
        entity.CreatedBy = Actor;
        entity.UpdatedAt = null;
        entity.UpdatedBy = null;

        var receiptIds = entity.ReceiptIds.Distinct().ToList();
        await _db.Deposits.AddAsync(entity);
        await _db.SaveChangesAsync();

        if (receiptIds.Count > 0)
        {
            await _db.DepositReceipts.AddRangeAsync(receiptIds.Select(receiptId => new DepositReceipt
            {
                DepositId = entity.Id,
                ReceiptId = receiptId
            }));
            await _db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        entity.ReceiptIds = receiptIds;
        return entity;
    }

    public async Task UpdateAsync(Deposit entity)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var existing = await _db.Deposits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == entity.Id)
            ?? throw new KeyNotFoundException($"Deposit #{entity.Id} tidak ditemukan.");

        entity.CreatedAt = existing.CreatedAt;
        entity.CreatedBy = existing.CreatedBy;
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = Actor;

        var receiptIds = entity.ReceiptIds.Distinct().ToList();

        // AddAsync pada request yang sama dapat meninggalkan entity/link masih tracked.
        // Clear tracker sebelum mengganti junction rows agar tidak terjadi duplicate tracked key.
        _db.ChangeTracker.Clear();

        await _db.DepositReceipts
            .Where(x => x.DepositId == entity.Id)
            .ExecuteDeleteAsync();

        _db.Deposits.Update(entity);

        if (receiptIds.Count > 0)
        {
            await _db.DepositReceipts.AddRangeAsync(receiptIds.Select(receiptId => new DepositReceipt
            {
                DepositId = entity.Id,
                ReceiptId = receiptId
            }));
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        entity.ReceiptIds = receiptIds;
    }

    public async Task DeleteAsync(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var links = await _db.DepositReceipts.Where(x => x.DepositId == id).ToListAsync();
        if (links.Count > 0)
            _db.DepositReceipts.RemoveRange(links);

        var deposit = await _db.Deposits.FirstOrDefaultAsync(x => x.Id == id);
        if (deposit != null)
            _db.Deposits.Remove(deposit);

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}

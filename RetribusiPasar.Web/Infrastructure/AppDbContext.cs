using Microsoft.EntityFrameworkCore;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Market> Markets => Set<Market>();
    public DbSet<RetributionType> RetributionTypes => Set<RetributionType>();
    public DbSet<Tariff> Tariffs => Set<Tariff>();
    public DbSet<Collector> Collectors => Set<Collector>();
    public DbSet<RetributionUnit> RetributionUnits => Set<RetributionUnit>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<MediaDistribution> MediaDistributions => Set<MediaDistribution>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Deposit> Deposits => Set<Deposit>();
    public DbSet<DepositReceipt> DepositReceipts => Set<DepositReceipt>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Market>(e =>
        {
            e.ToTable("MstMarket");
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RetributionType>(e =>
        {
            e.ToTable("MstRetributionType");
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Tariff>(e =>
        {
            e.ToTable("MstTariff");
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.MarketId, x.RetributionTypeId, x.EffectiveFrom });
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RetributionType>().WithMany().HasForeignKey(x => x.RetributionTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Collector>(e =>
        {
            e.ToTable("MstCollector");
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RetributionUnit>(e =>
        {
            e.ToTable("MstRetributionUnit");
            e.HasIndex(x => new { x.MarketId, x.Code }).IsUnique();
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RetributionType>().WithMany().HasForeignKey(x => x.RetributionTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Collector>().WithMany().HasForeignKey(x => x.CollectorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockBatch>(e =>
        {
            e.ToTable("TrxStockBatch");
            e.Ignore(x => x.TotalSheets);
            e.HasIndex(x => new { x.MediaType, x.StartSerial, x.EndSerial });
        });

        modelBuilder.Entity<MediaDistribution>(e =>
        {
            e.ToTable("TrxMediaDistribution");
            e.Ignore(x => x.TotalSheets);
            e.HasIndex(x => x.DistributionNo).IsUnique();
            e.HasIndex(x => new { x.MediaType, x.StartSerial, x.EndSerial });
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Receipt>(e =>
        {
            e.ToTable("TrxReceipt");
            e.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            e.Property(x => x.ReceivedAmount).HasPrecision(18, 2);
            e.HasIndex(x => x.TransactionNo).IsUnique();
            e.HasIndex(x => new { x.ReceiptDate, x.MarketId });
            e.HasIndex(x => new { x.StartSerial, x.EndSerial });
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RetributionUnit>().WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RetributionType>().WithMany().HasForeignKey(x => x.RetributionTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Collector>().WithMany().HasForeignKey(x => x.CollectorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Deposit>(e =>
        {
            e.ToTable("TrxDeposit");
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Ignore(x => x.ReceiptIds);
            e.HasIndex(x => x.DepositNo).IsUnique();
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DepositReceipt>(e =>
        {
            e.ToTable("TrxDepositReceipt");
            e.HasKey(x => new { x.DepositId, x.ReceiptId });
            e.HasIndex(x => x.ReceiptId).IsUnique();
            e.HasOne<Deposit>().WithMany().HasForeignKey(x => x.DepositId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Receipt>().WithMany().HasForeignKey(x => x.ReceiptId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("AppUser");
            e.Ignore(x => x.NewPassword);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasOne<Market>().WithMany().HasForeignKey(x => x.MarketId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AccountingPeriod>(e =>
        {
            e.ToTable("AccountingPeriod");
            e.Property(x => x.Period).HasMaxLength(7);
            e.HasIndex(x => x.Period).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLog");
            e.Property(x => x.UserName).HasMaxLength(80);
            e.Property(x => x.Action).HasMaxLength(30);
            e.Property(x => x.EntityName).HasMaxLength(80);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasIndex(x => new { x.EntityName, x.EntityId, x.Timestamp });
        });
    }
}

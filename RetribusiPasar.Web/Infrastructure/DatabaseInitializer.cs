using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var autoCreate = configuration.GetValue("Database:AutoCreate", true);
        var seedDemoData = configuration.GetValue("Database:SeedDemoData", true);

        try
        {
            if (autoCreate)
                await db.Database.EnsureCreatedAsync();

            if (seedDemoData)
                await SeedAsync(db, configuration);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database initialization failed. Check ConnectionStrings:DefaultConnection and SQL Server permissions.");
            throw;
        }
    }

    private static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
    {
        if (await db.Markets.AnyAsync())
            return;

        await using var tx = await db.Database.BeginTransactionAsync();

        var panorama = new Market { Code="PSR001", Name="Pasar Panorama", Type="Pasar", Address="Kota Bengkulu", CreatedBy="seed" };
        var minggu = new Market { Code="PSR002", Name="Pasar Minggu", Type="Pasar", Address="Kota Bengkulu", CreatedBy="seed" };
        var barukoto = new Market { Code="PSR003", Name="Pasar Barukoto", Type="Pasar", Address="Kota Bengkulu", CreatedBy="seed" };
        var ruko1 = new Market { Code="RKO001", Name="Ruko KZ Abidin 1", Type="Ruko", Address="Kota Bengkulu", CreatedBy="seed" };
        var ruko2 = new Market { Code="RKO002", Name="Ruko KZ Abidin 2", Type="Ruko", Address="Kota Bengkulu", CreatedBy="seed" };
        db.Markets.AddRange(panorama, minggu, barukoto, ruko1, ruko2);
        await db.SaveChangesAsync();

        var kios = new RetributionType { Code="KIOS", Name="Kios", MediaType="STRD", BillingCycle="Bulanan", CreatedBy="seed" };
        var los = new RetributionType { Code="LOS", Name="Los/Auning", MediaType="STRD", BillingCycle="Bulanan", CreatedBy="seed" };
        var pelataran = new RetributionType { Code="PELAT", Name="Pelataran", MediaType="Karcis", BillingCycle="PerTransaksi", CreatedBy="seed" };
        var ruko = new RetributionType { Code="RUKO", Name="Sewa Ruko", MediaType="Tanpa Media", BillingCycle="Bulanan", CreatedBy="seed" };
        db.RetributionTypes.AddRange(kios, los, pelataran, ruko);
        await db.SaveChangesAsync();

        db.Tariffs.AddRange(
            new Tariff { MarketId=panorama.Id, RetributionTypeId=kios.Id, Amount=25000, EffectiveFrom=new DateTime(2026,7,1), CreatedBy="seed" },
            new Tariff { MarketId=panorama.Id, RetributionTypeId=los.Id, Amount=20000, EffectiveFrom=new DateTime(2026,7,1), CreatedBy="seed" },
            new Tariff { MarketId=panorama.Id, RetributionTypeId=pelataran.Id, Amount=5000, EffectiveFrom=new DateTime(2026,7,1), CreatedBy="seed" },
            new Tariff { MarketId=minggu.Id, RetributionTypeId=kios.Id, Amount=25000, EffectiveFrom=new DateTime(2026,7,1), CreatedBy="seed" },
            new Tariff { MarketId=minggu.Id, RetributionTypeId=pelataran.Id, Amount=5000, EffectiveFrom=new DateTime(2026,7,1), CreatedBy="seed" }
        );

        var ahmad = new Collector { Code="PTG001", Name="Ahmad", MarketId=panorama.Id, Phone="081200000001", CreatedBy="seed" };
        var budi = new Collector { Code="PTG002", Name="Budi", MarketId=minggu.Id, Phone="081200000002", CreatedBy="seed" };
        db.Collectors.AddRange(ahmad, budi);
        await db.SaveChangesAsync();

        var unitA1 = new RetributionUnit { MarketId=panorama.Id, Code="A-01", RetributionTypeId=kios.Id, PayerName="Ibu Sari", CollectorId=ahmad.Id, Status="Berfungsi", EffectiveFrom=new DateTime(2026,1,1), CreatedBy="seed" };
        var unitA2 = new RetributionUnit { MarketId=panorama.Id, Code="A-02", RetributionTypeId=los.Id, PayerName="Bpk Andi", CollectorId=ahmad.Id, Status="Berfungsi", EffectiveFrom=new DateTime(2026,1,1), CreatedBy="seed" };
        var unitB1 = new RetributionUnit { MarketId=minggu.Id, Code="B-01", RetributionTypeId=kios.Id, PayerName="Ibu Nia", CollectorId=budi.Id, Status="Berfungsi", EffectiveFrom=new DateTime(2026,2,1), CreatedBy="seed" };
        db.RetributionUnits.AddRange(unitA1, unitA2, unitB1);

        db.StockBatches.AddRange(
            new StockBatch { MediaType="STRD", ReceivedDate=new DateTime(2026,10,1), StartSerial=120001, EndSerial=124999, DocumentNo="BA-STK/2026/001", CreatedBy="seed" },
            new StockBatch { MediaType="Karcis", ReceivedDate=new DateTime(2026,10,1), StartSerial=130001, EndSerial=139999, DocumentNo="BA-STK/2026/002", CreatedBy="seed" }
        );
        await db.SaveChangesAsync();

        db.MediaDistributions.AddRange(
            new MediaDistribution { DistributionNo="BA/2026/00112", MarketId=panorama.Id, MediaType="STRD", DistributionDate=new DateTime(2026,10,3), StartSerial=120001, EndSerial=120800, ReceiverName="Kepala Pasar Panorama", CreatedBy="seed" },
            new MediaDistribution { DistributionNo="BA/2026/00130", MarketId=minggu.Id, MediaType="Karcis", DistributionDate=new DateTime(2026,10,6), StartSerial=130201, EndSerial=130400, ReceiverName="Kepala Pasar Minggu", CreatedBy="seed" }
        );
        await db.SaveChangesAsync();

        var receipt1 = new Receipt { TransactionNo="TRX/2026/001248", ReceiptDate=new DateTime(2026,10,7), MarketId=panorama.Id, UnitId=unitA1.Id, RetributionTypeId=kios.Id, CollectorId=ahmad.Id, StartSerial=120501, EndSerial=120501, PeriodStart=new DateTime(2026,10,1), Months=1, ExpectedAmount=25000, ReceivedAmount=25000, PaymentMethod="Tunai", Status="ACTIVE", CreatedBy="seed" };
        var receipt2 = new Receipt { TransactionNo="TRX/2026/001247", ReceiptDate=new DateTime(2026,10,7), MarketId=minggu.Id, RetributionTypeId=pelataran.Id, CollectorId=budi.Id, StartSerial=130210, EndSerial=130214, PeriodStart=new DateTime(2026,10,1), Months=1, ExpectedAmount=25000, ReceivedAmount=25000, PaymentMethod="Tunai", Status="ACTIVE", CreatedBy="seed" };
        db.Receipts.AddRange(receipt1, receipt2);
        await db.SaveChangesAsync();

        var deposit = new Deposit
        {
            DepositNo="STS/2026/00127", DepositDate=new DateTime(2026,10,7), MarketId=panorama.Id,
            FromDate=new DateTime(2026,10,1), ToDate=new DateTime(2026,10,7),
            BankName="Bank Daerah - Rekening Penerimaan", Amount=25000, Status="VERIFIED", CreatedBy="seed"
        };
        db.Deposits.Add(deposit);
        await db.SaveChangesAsync();
        db.DepositReceipts.Add(new DepositReceipt { DepositId=deposit.Id, ReceiptId=receipt1.Id });

        var adminPassword = configuration["Seed:AdminPassword"] ?? "demo123";
        var operatorPassword = configuration["Seed:OperatorPassword"] ?? "demo123";
        var hasher = new PasswordHasher<AppUser>();

        var admin = new AppUser { Username="admin.retribusi", DisplayName="Administrator Sistem", Role="Administrator", CreatedBy="seed" };
        admin.PasswordHash = hasher.HashPassword(admin, adminPassword);

        var oper = new AppUser { Username="operator.panorama", DisplayName="Operator Panorama", Role="Operator", MarketId=panorama.Id, CreatedBy="seed" };
        oper.PasswordHash = hasher.HashPassword(oper, operatorPassword);

        db.AppUsers.AddRange(admin, oper);
        db.AccountingPeriods.AddRange(
            new AccountingPeriod { Period="2026-09", Status="CLOSED", ClosedAt=new DateTime(2026,10,3,17,10,0), ClosedBy="admin.retribusi", CreatedBy="seed" },
            new AccountingPeriod { Period="2026-10", Status="OPEN", CreatedBy="seed" }
        );

        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}

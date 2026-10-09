using RetribusiPasar.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace RetribusiPasar.Web.Infrastructure;

public static class SeedData
{
    public static void Initialize(InMemoryDataStore store)
    {
        lock (store.SyncRoot)
        {
            if (store.Set<Market>().Any()) return;

            store.Set<Market>().AddRange(new[]
            {
                new Market { Id=1, Code="PSR001", Name="Pasar Panorama", Type="Pasar", Address="Kota Bengkulu" },
                new Market { Id=2, Code="PSR002", Name="Pasar Minggu", Type="Pasar", Address="Kota Bengkulu" },
                new Market { Id=3, Code="PSR003", Name="Pasar Barukoto", Type="Pasar", Address="Kota Bengkulu" },
                new Market { Id=4, Code="RKO001", Name="Ruko KZ Abidin 1", Type="Ruko", Address="Kota Bengkulu" },
                new Market { Id=5, Code="RKO002", Name="Ruko KZ Abidin 2", Type="Ruko", Address="Kota Bengkulu" }
            });
            store.SetSequence<Market>(5);

            store.Set<RetributionType>().AddRange(new[]
            {
                new RetributionType { Id=1, Code="KIOS", Name="Kios", MediaType="STRD", BillingCycle="Bulanan" },
                new RetributionType { Id=2, Code="LOS", Name="Los/Auning", MediaType="STRD", BillingCycle="Bulanan" },
                new RetributionType { Id=3, Code="PELAT", Name="Pelataran", MediaType="Karcis", BillingCycle="PerTransaksi" },
                new RetributionType { Id=4, Code="RUKO", Name="Sewa Ruko", MediaType="Tanpa Media", BillingCycle="Bulanan" }
            });
            store.SetSequence<RetributionType>(4);

            store.Set<Tariff>().AddRange(new[]
            {
                new Tariff { Id=1, MarketId=1, RetributionTypeId=1, Amount=25000, EffectiveFrom=new DateTime(2026,7,1) },
                new Tariff { Id=2, MarketId=1, RetributionTypeId=2, Amount=20000, EffectiveFrom=new DateTime(2026,7,1) },
                new Tariff { Id=3, MarketId=1, RetributionTypeId=3, Amount=5000, EffectiveFrom=new DateTime(2026,7,1) },
                new Tariff { Id=4, MarketId=2, RetributionTypeId=1, Amount=25000, EffectiveFrom=new DateTime(2026,7,1) }
            });
            store.SetSequence<Tariff>(4);

            store.Set<Collector>().AddRange(new[]
            {
                new Collector { Id=1, Code="PTG001", Name="Ahmad", MarketId=1, Phone="081200000001" },
                new Collector { Id=2, Code="PTG002", Name="Budi", MarketId=2, Phone="081200000002" }
            });
            store.SetSequence<Collector>(2);

            store.Set<RetributionUnit>().AddRange(new[]
            {
                new RetributionUnit { Id=1, MarketId=1, Code="A-01", RetributionTypeId=1, PayerName="Ibu Sari", CollectorId=1, Status="Berfungsi", EffectiveFrom=new DateTime(2026,1,1) },
                new RetributionUnit { Id=2, MarketId=1, Code="A-02", RetributionTypeId=2, PayerName="Bpk Andi", CollectorId=1, Status="Berfungsi", EffectiveFrom=new DateTime(2026,1,1) },
                new RetributionUnit { Id=3, MarketId=2, Code="B-01", RetributionTypeId=1, PayerName="Ibu Nia", CollectorId=2, Status="Berfungsi", EffectiveFrom=new DateTime(2026,2,1) }
            });
            store.SetSequence<RetributionUnit>(3);

            store.Set<StockBatch>().AddRange(new[]
            {
                new StockBatch { Id=1, MediaType="STRD", ReceivedDate=new DateTime(2026,10,1), StartSerial=120001, EndSerial=124999, DocumentNo="BA-STK/2026/001" },
                new StockBatch { Id=2, MediaType="Karcis", ReceivedDate=new DateTime(2026,10,1), StartSerial=130001, EndSerial=139999, DocumentNo="BA-STK/2026/002" }
            });
            store.SetSequence<StockBatch>(2);

            store.Set<MediaDistribution>().AddRange(new[]
            {
                new MediaDistribution { Id=1, DistributionNo="BA/2026/00112", MarketId=1, MediaType="STRD", DistributionDate=new DateTime(2026,10,3), StartSerial=120001, EndSerial=120800, ReceiverName="Kepala Pasar Panorama" },
                new MediaDistribution { Id=2, DistributionNo="BA/2026/00130", MarketId=2, MediaType="Karcis", DistributionDate=new DateTime(2026,10,6), StartSerial=130201, EndSerial=130400, ReceiverName="Kepala Pasar Minggu" }
            });
            store.SetSequence<MediaDistribution>(2);

            store.Set<Receipt>().AddRange(new[]
            {
                new Receipt { Id=1, TransactionNo="TRX/2026/001248", ReceiptDate=new DateTime(2026,10,7), MarketId=1, UnitId=1, RetributionTypeId=1, CollectorId=1, StartSerial=120501, EndSerial=120501, PeriodStart=new DateTime(2026,10,1), Months=1, ExpectedAmount=25000, ReceivedAmount=25000, PaymentMethod="Tunai", Status="ACTIVE" },
                new Receipt { Id=2, TransactionNo="TRX/2026/001247", ReceiptDate=new DateTime(2026,10,7), MarketId=2, RetributionTypeId=3, CollectorId=2, StartSerial=130210, EndSerial=130214, PeriodStart=new DateTime(2026,10,1), Months=1, ExpectedAmount=25000, ReceivedAmount=25000, PaymentMethod="Tunai", Status="ACTIVE" }
            });
            store.SetSequence<Receipt>(2);

            store.Set<Deposit>().Add(new Deposit
            {
                Id=1, DepositNo="STS/2026/00127", DepositDate=new DateTime(2026,10,6), MarketId=1,
                FromDate=new DateTime(2026,10,1), ToDate=new DateTime(2026,10,7), BankName="Bank Daerah - Rekening Penerimaan",
                Amount=25000, Status="VERIFIED", ReceiptIds=new List<int>{1}
            });
            store.SetSequence<Deposit>(1);

            var hasher = new PasswordHasher<AppUser>();
            var admin = new AppUser { Id=1, Username="admin.retribusi", DisplayName="Administrator Sistem", Role="Administrator" };
            admin.PasswordHash = hasher.HashPassword(admin, "demo123");
            var oper = new AppUser { Id=2, Username="operator.panorama", DisplayName="Operator Panorama", Role="Operator", MarketId=1 };
            oper.PasswordHash = hasher.HashPassword(oper, "demo123");
            store.Set<AppUser>().AddRange(new[] { admin, oper });
            store.SetSequence<AppUser>(2);

            store.Set<AccountingPeriod>().AddRange(new[]
            {
                new AccountingPeriod { Id=1, Period="2026-09", Status="CLOSED", ClosedAt=new DateTime(2026,10,3,17,10,0), ClosedBy="admin.retribusi" },
                new AccountingPeriod { Id=2, Period="2026-10", Status="OPEN" }
            });
            store.SetSequence<AccountingPeriod>(2);
        }
    }
}

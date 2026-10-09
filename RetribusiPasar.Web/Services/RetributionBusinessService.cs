using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Models.ViewModels;

namespace RetribusiPasar.Web.Services;

public class RetributionBusinessService
{
    private readonly IRepository<StockBatch> _stocks;
    private readonly IRepository<MediaDistribution> _distributions;
    private readonly IRepository<Receipt> _receipts;
    private readonly IRepository<Tariff> _tariffs;
    private readonly IRepository<RetributionType> _types;
    private readonly IRepository<AccountingPeriod> _periods;
    private readonly IRepository<Deposit> _deposits;
    private readonly IRepository<RetributionUnit> _units;
    private readonly IRepository<Market> _markets;
    private readonly IRepository<Collector> _collectors;

    public RetributionBusinessService(
        IRepository<StockBatch> stocks,
        IRepository<MediaDistribution> distributions,
        IRepository<Receipt> receipts,
        IRepository<Tariff> tariffs,
        IRepository<RetributionType> types,
        IRepository<AccountingPeriod> periods,
        IRepository<Deposit> deposits,
        IRepository<RetributionUnit> units,
        IRepository<Market> markets,
        IRepository<Collector> collectors)
    {
        _stocks = stocks;
        _distributions = distributions;
        _receipts = receipts;
        _tariffs = tariffs;
        _types = types;
        _periods = periods;
        _deposits = deposits;
        _units = units;
        _markets = markets;
        _collectors = collectors;
    }

    public static string SerialText(int? start, int? end)
        => start.HasValue
            ? (end.HasValue && end.Value != start.Value
                ? $"{start.Value:000000}-{end.Value:000000}"
                : $"{start.Value:000000}")
            : "-";

    public async Task<string?> ValidateStockAsync(StockBatch model, int ignoreId = 0)
    {
        if (model.EndSerial < model.StartSerial)
            return "Seri akhir tidak boleh lebih kecil dari seri awal.";

        var all = await _stocks.GetAllAsync();
        if (all.Any(x => x.Id != ignoreId &&
                         x.IsActive &&
                         x.MediaType == model.MediaType &&
                         model.StartSerial <= x.EndSerial &&
                         model.EndSerial >= x.StartSerial))
            return "Range nomor seri bertumpuk dengan stok yang sudah ada.";

        return null;
    }

    public async Task<string?> ValidateDistributionAsync(MediaDistribution model, int ignoreId = 0)
    {
        if (model.EndSerial < model.StartSerial)
            return "Seri akhir tidak boleh lebih kecil dari seri awal.";

        var stocks = (await _stocks.GetAllAsync())
            .Where(x => x.IsActive && x.MediaType == model.MediaType)
            .OrderBy(x => x.StartSerial)
            .ToList();

        if (!stocks.Any())
            return $"Belum ada stok aktif untuk media {model.MediaType}.";

        var covered = stocks.Any(x =>
            model.StartSerial >= x.StartSerial &&
            model.EndSerial <= x.EndSerial);

        if (!covered)
        {
            var ranges = string.Join(", ", stocks.Select(x => $"{x.StartSerial:000000}-{x.EndSerial:000000}"));
            return $"Range {model.StartSerial:000000}-{model.EndSerial:000000} tidak tersedia pada stok {model.MediaType}. Stok tersedia: {ranges}.";
        }

        var dists = await _distributions.GetAllAsync();
        var overlap = dists.FirstOrDefault(x =>
            x.Id != ignoreId &&
            x.IsActive &&
            x.MediaType == model.MediaType &&
            model.StartSerial <= x.EndSerial &&
            model.EndSerial >= x.StartSerial);

        if (overlap != null)
            return $"Range bertumpuk dengan distribusi {overlap.DistributionNo}, seri {overlap.StartSerial:000000}-{overlap.EndSerial:000000}.";

        var receipts = await _receipts.GetAllAsync();
        var types = (await _types.GetAllAsync()).ToDictionary(x => x.Id, x => x.MediaType);

        if (receipts.Any(x =>
            x.Status == "ACTIVE" &&
            x.StartSerial.HasValue &&
            x.EndSerial.HasValue &&
            types.GetValueOrDefault(x.RetributionTypeId) == model.MediaType &&
            model.StartSerial <= x.EndSerial.Value &&
            model.EndSerial >= x.StartSerial.Value))
            return "Sebagian nomor seri sudah digunakan pada penerimaan.";

        return null;
    }

    public async Task ApplyUnitDefaultsAsync(Receipt model)
    {
        if (!model.UnitId.HasValue)
            return;

        var unit = await _units.GetByIdAsync(model.UnitId.Value);
        if (unit == null || !unit.IsActive)
            return;

        model.MarketId = unit.MarketId;
        model.RetributionTypeId = unit.RetributionTypeId;
        model.CollectorId = unit.CollectorId;
    }

    public async Task<decimal> GetTariffAsync(int marketId, int retributionTypeId, DateTime periodStart)
    {
        var tariffs = await _tariffs.GetAllAsync();

        return tariffs
            .Where(x =>
                x.IsActive &&
                x.MarketId == marketId &&
                x.RetributionTypeId == retributionTypeId &&
                x.EffectiveFrom.Date <= periodStart.Date &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value.Date >= periodStart.Date))
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefault()?.Amount ?? 0;
    }

    public async Task<decimal> CalculateExpectedAmountAsync(Receipt model)
    {
        var types = await _types.GetAllAsync();
        var type = types.FirstOrDefault(x => x.Id == model.RetributionTypeId);
        var tariff = await GetTariffAsync(model.MarketId, model.RetributionTypeId, model.PeriodStart);

        if (type?.BillingCycle == "PerTransaksi")
        {
            var count = model.StartSerial.HasValue
                ? Math.Max(1, (model.EndSerial ?? model.StartSerial).Value - model.StartSerial.Value + 1)
                : 1;

            return tariff * count;
        }

        return tariff * Math.Max(1, model.Months);
    }

    public async Task<List<SerialRangeViewModel>> GetAvailableSerialRangesAsync(
        int marketId,
        int retributionTypeId,
        int ignoreReceiptId = 0)
    {
        var type = (await _types.GetAllAsync()).FirstOrDefault(x => x.Id == retributionTypeId);
        if (type == null || type.MediaType == "Tanpa Media")
            return new();

        var distributed = (await _distributions.GetAllAsync())
            .Where(x => x.IsActive &&
                        x.MarketId == marketId &&
                        x.MediaType == type.MediaType)
            .OrderBy(x => x.StartSerial)
            .Select(x => (Start: x.StartSerial, End: x.EndSerial))
            .ToList();

        if (!distributed.Any())
            return new();

        var typeMedia = (await _types.GetAllAsync()).ToDictionary(x => x.Id, x => x.MediaType);

        var used = (await _receipts.GetAllAsync())
            .Where(x => x.Id != ignoreReceiptId &&
                        x.Status == "ACTIVE" &&
                        x.StartSerial.HasValue &&
                        x.EndSerial.HasValue &&
                        typeMedia.GetValueOrDefault(x.RetributionTypeId) == type.MediaType)
            .Select(x => (Start: x.StartSerial!.Value, End: x.EndSerial!.Value))
            .OrderBy(x => x.Start)
            .ToList();

        var result = new List<SerialRangeViewModel>();

        foreach (var dist in distributed)
        {
            var segments = new List<(int Start, int End)> { dist };

            foreach (var u in used)
            {
                var next = new List<(int Start, int End)>();

                foreach (var s in segments)
                {
                    if (u.End < s.Start || u.Start > s.End)
                    {
                        next.Add(s);
                        continue;
                    }

                    if (u.Start > s.Start)
                        next.Add((s.Start, u.Start - 1));

                    if (u.End < s.End)
                        next.Add((u.End + 1, s.End));
                }

                segments = next;
                if (!segments.Any())
                    break;
            }

            result.AddRange(segments
                .Where(x => x.End >= x.Start)
                .Select(x => new SerialRangeViewModel
                {
                    StartSerial = x.Start,
                    EndSerial = x.End
                }));
        }

        return result
            .OrderBy(x => x.StartSerial)
            .ToList();
    }

    public async Task<ReceiptFormContextViewModel?> BuildReceiptFormContextAsync(
        int? unitId,
        int? marketId,
        int? retributionTypeId,
        int? collectorId,
        DateTime periodStart,
        int months,
        int? startSerial,
        int? endSerial,
        int ignoreReceiptId = 0)
    {
        RetributionUnit? unit = null;

        if (unitId.HasValue)
        {
            unit = await _units.GetByIdAsync(unitId.Value);
            if (unit == null || !unit.IsActive)
                return null;

            marketId = unit.MarketId;
            retributionTypeId = unit.RetributionTypeId;
            collectorId = unit.CollectorId;
        }

        if (!marketId.HasValue || marketId.Value <= 0 ||
            !retributionTypeId.HasValue || retributionTypeId.Value <= 0)
            return null;

        var markets = await _markets.GetAllAsync();
        var types = await _types.GetAllAsync();
        var collectors = await _collectors.GetAllAsync();

        var market = markets.FirstOrDefault(x => x.Id == marketId.Value);
        var type = types.FirstOrDefault(x => x.Id == retributionTypeId.Value);

        if (market == null || type == null)
            return null;

        var tempReceipt = new Receipt
        {
            MarketId = marketId.Value,
            RetributionTypeId = retributionTypeId.Value,
            PeriodStart = periodStart,
            Months = Math.Max(1, months),
            StartSerial = startSerial,
            EndSerial = endSerial
        };

        var tariff = await GetTariffAsync(marketId.Value, retributionTypeId.Value, periodStart);
        var expected = await CalculateExpectedAmountAsync(tempReceipt);
        var available = await GetAvailableSerialRangesAsync(marketId.Value, retributionTypeId.Value, ignoreReceiptId);

        return new ReceiptFormContextViewModel
        {
            UnitId = unit?.Id,
            MarketId = marketId.Value,
            MarketName = market.Name,
            RetributionTypeId = retributionTypeId.Value,
            RetributionTypeName = type.Name,
            CollectorId = collectorId,
            CollectorName = collectorId.HasValue
                ? collectors.FirstOrDefault(x => x.Id == collectorId.Value)?.Name ?? "-"
                : "-",
            MediaType = type.MediaType,
            Tariff = tariff,
            ExpectedAmount = expected,
            AvailableRanges = available
        };
    }

    public async Task<string?> ValidateReceiptAsync(Receipt model, int ignoreId = 0)
    {
        var periods = await _periods.GetAllAsync();
        var period = periods.FirstOrDefault(x => x.Period == model.ReceiptDate.ToString("yyyy-MM"));

        if (period?.Status == "CLOSED")
            return $"Periode {period.Period} sudah ditutup.";

        var types = await _types.GetAllAsync();
        var type = types.FirstOrDefault(x => x.Id == model.RetributionTypeId);

        if (type == null)
            return "Jenis retribusi tidak valid.";

        if (model.UnitId.HasValue)
        {
            var unit = await _units.GetByIdAsync(model.UnitId.Value);
            if (unit == null || !unit.IsActive)
                return "Unit tidak valid atau sudah tidak aktif.";

            if (unit.MarketId != model.MarketId ||
                unit.RetributionTypeId != model.RetributionTypeId ||
                unit.CollectorId != model.CollectorId)
                return "Data Pasar/Jenis/Petugas tidak sesuai dengan unit yang dipilih.";
        }
        else if (model.CollectorId.HasValue)
        {
            var collector = await _collectors.GetByIdAsync(model.CollectorId.Value);
            if (collector == null || !collector.IsActive || collector.MarketId != model.MarketId)
                return "Petugas yang dipilih tidak terdaftar pada pasar tersebut.";
        }

        if (type.MediaType != "Tanpa Media")
        {
            if (!model.StartSerial.HasValue)
                return $"Nomor seri {type.MediaType} wajib diisi.";

            model.EndSerial ??= model.StartSerial;

            if (model.EndSerial.Value < model.StartSerial.Value)
                return "Seri akhir tidak boleh lebih kecil dari seri awal.";

            var dists = await _distributions.GetAllAsync();

            var allocated = dists.Any(x =>
                x.IsActive &&
                x.MarketId == model.MarketId &&
                x.MediaType == type.MediaType &&
                model.StartSerial.Value >= x.StartSerial &&
                model.EndSerial.Value <= x.EndSerial);

            if (!allocated)
            {
                var market = (await _markets.GetAllAsync()).FirstOrDefault(x => x.Id == model.MarketId)?.Name ?? "pasar terpilih";
                var ranges = dists
                    .Where(x => x.IsActive && x.MarketId == model.MarketId && x.MediaType == type.MediaType)
                    .OrderBy(x => x.StartSerial)
                    .Select(x => $"{x.StartSerial:000000}-{x.EndSerial:000000}")
                    .ToList();

                var availableText = ranges.Any()
                    ? string.Join(", ", ranges)
                    : "belum ada distribusi";

                return $"Nomor seri {type.MediaType} belum dialokasikan ke {market}. Range distribusi {type.MediaType}: {availableText}.";
            }

            var receipts = await _receipts.GetAllAsync();
            var typeMedia = types.ToDictionary(x => x.Id, x => x.MediaType);

            if (receipts.Any(x =>
                x.Id != ignoreId &&
                x.Status == "ACTIVE" &&
                x.StartSerial.HasValue &&
                x.EndSerial.HasValue &&
                typeMedia.GetValueOrDefault(x.RetributionTypeId) == type.MediaType &&
                model.StartSerial.Value <= x.EndSerial.Value &&
                model.EndSerial.Value >= x.StartSerial.Value))
                return "Sebagian nomor seri sudah digunakan pada transaksi lain.";
        }
        else
        {
            model.StartSerial = null;
            model.EndSerial = null;
        }

        return null;
    }

    public async Task<HashSet<int>> DepositedReceiptIdsAsync(int ignoreDepositId = 0)
    {
        var deposits = await _deposits.GetAllAsync();

        return deposits
            .Where(x => x.Id != ignoreDepositId && x.Status != "VOID")
            .SelectMany(x => x.ReceiptIds)
            .ToHashSet();
    }
}

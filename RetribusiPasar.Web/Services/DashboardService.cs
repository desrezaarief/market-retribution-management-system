using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Models.ViewModels;

namespace RetribusiPasar.Web.Services;

public class DashboardService
{
    private readonly IRepository<Market> _markets;
    private readonly IRepository<Receipt> _receipts;
    private readonly IRepository<Deposit> _deposits;
    private readonly IRepository<RetributionUnit> _units;
    private readonly IRepository<Tariff> _tariffs;
    private readonly ArrearsService _arrears;

    public DashboardService(IRepository<Market> markets, IRepository<Receipt> receipts,
        IRepository<Deposit> deposits, IRepository<RetributionUnit> units, IRepository<Tariff> tariffs, ArrearsService arrears)
    {
        _markets=markets; _receipts=receipts; _deposits=deposits; _units=units; _tariffs=tariffs; _arrears=arrears;
    }

    public async Task<DashboardViewModel> BuildAsync(string? period = null)
    {
        period ??= DateTime.Today.ToString("yyyy-MM");
        var markets = await _markets.GetAllAsync();
        var receipts = (await _receipts.GetAllAsync()).Where(x=>x.Status=="ACTIVE" && x.ReceiptDate.ToString("yyyy-MM")==period).ToList();
        var deposits = (await _deposits.GetAllAsync()).Where(x=>x.Status!="VOID" && x.DepositDate.ToString("yyyy-MM")==period).ToList();
        var units = await _units.GetAllAsync();
        var tariffs = await _tariffs.GetAllAsync();

        var vm = new DashboardViewModel
        {
            TotalReceipt = receipts.Sum(x=>x.ReceivedAmount),
            TotalDeposited = deposits.Sum(x=>x.Amount),
            UsedMedia = receipts.Sum(x=> x.StartSerial.HasValue ? Math.Max(1,(x.EndSerial??x.StartSerial).Value-x.StartSerial.Value+1) : 0),
            TotalArrears = (await _arrears.BuildAsync(period)).TotalAmount
        };

        foreach (var m in markets.Where(x=>x.IsActive))
        {
            vm.Markets.Add(new MarketSummaryRow
            {
                MarketId=m.Id,
                MarketName=m.Name,
                Receipt=receipts.Where(x=>x.MarketId==m.Id).Sum(x=>x.ReceivedAmount),
                Deposit=deposits.Where(x=>x.MarketId==m.Id).Sum(x=>x.Amount),
                Arrears=0
            });
        }
        return vm;
    }
}

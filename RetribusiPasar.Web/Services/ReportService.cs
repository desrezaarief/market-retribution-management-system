using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Models.ViewModels;

namespace RetribusiPasar.Web.Services;

public class ReportService
{
    private readonly IRepository<Market> _markets;
    private readonly IRepository<Receipt> _receipts;
    private readonly IRepository<Deposit> _deposits;

    public ReportService(IRepository<Market> markets, IRepository<Receipt> receipts, IRepository<Deposit> deposits)
    {
        _markets=markets; _receipts=receipts; _deposits=deposits;
    }

    public async Task<ReportViewModel> BuildAsync(string period, int? marketId)
    {
        var vm = new ReportViewModel { Period=period, MarketId=marketId };
        var markets = (await _markets.GetAllAsync()).Where(x=>x.IsActive && (!marketId.HasValue || x.Id==marketId)).ToList();
        var receipts = (await _receipts.GetAllAsync()).Where(x=>x.Status=="ACTIVE" && x.ReceiptDate.ToString("yyyy-MM")==period).ToList();
        var deposits = (await _deposits.GetAllAsync()).Where(x=>x.Status!="VOID" && x.DepositDate.ToString("yyyy-MM")==period).ToList();

        vm.Rows = markets.Select(m=>new ReportRow
        {
            MarketName=m.Name,
            Receipt=receipts.Where(x=>x.MarketId==m.Id).Sum(x=>x.ReceivedAmount),
            Deposit=deposits.Where(x=>x.MarketId==m.Id).Sum(x=>x.Amount)
        }).ToList();
        return vm;
    }
}

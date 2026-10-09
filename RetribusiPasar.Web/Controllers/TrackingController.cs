using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Helpers;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Models.ViewModels;

namespace RetribusiPasar.Web.Controllers;

public class TrackingController : Controller
{
    private readonly IRepository<StockBatch> _stocks;
    private readonly IRepository<MediaDistribution> _dists;
    private readonly IRepository<Receipt> _receipts;
    private readonly IRepository<Deposit> _deposits;
    private readonly IRepository<Market> _markets;

    public TrackingController(IRepository<StockBatch> stocks, IRepository<MediaDistribution> dists,
        IRepository<Receipt> receipts, IRepository<Deposit> deposits, IRepository<Market> markets)
    {
        _stocks=stocks; _dists=dists; _receipts=receipts; _deposits=deposits; _markets=markets;
    }

    public async Task<IActionResult> Index(int? serial)
    {
        var vm=new TrackingViewModel{Serial=serial};
        if(!serial.HasValue) return View(vm);

        var s = serial.Value;
        var stock=(await _stocks.GetAllAsync()).FirstOrDefault(x=>s>=x.StartSerial&&s<=x.EndSerial);
        if(stock==null){vm.Error="Nomor seri tidak ditemukan pada stok.";return View(vm);}

        vm.MediaType=stock.MediaType;
        vm.Status="IN_WAREHOUSE";
        vm.Events.Add(new(){Date=stock.ReceivedDate,Title="Diterima dari Dinas",Detail=stock.DocumentNo??"Penerimaan stok"});

        var markets=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);
        var dist=(await _dists.GetAllAsync())
            .Where(x=>x.MediaType==stock.MediaType&&s>=x.StartSerial&&s<=x.EndSerial)
            .OrderByDescending(x=>x.DistributionDate).FirstOrDefault();

        if(dist!=null)
        {
            vm.Status="DISTRIBUTED";
            vm.Events.Add(new(){Date=dist.DistributionDate,Title=$"Didistribusikan ke {markets.GetValueOrDefault(dist.MarketId,"Pasar")}",Detail=dist.DistributionNo});
        }

        var receipt=(await _receipts.GetAllAsync())
            .Where(x=>x.Status=="ACTIVE"&&x.StartSerial.HasValue&&x.EndSerial.HasValue&&s>=x.StartSerial.Value&&s<=x.EndSerial.Value)
            .OrderByDescending(x=>x.ReceiptDate).FirstOrDefault();

        if(receipt!=null)
        {
            vm.Status="USED";
            vm.Events.Add(new(){Date=receipt.ReceiptDate,Title="Digunakan pada penerimaan",Detail=$"{receipt.TransactionNo} - {FormatHelper.Rupiah(receipt.ReceivedAmount)}"});
            var dep=(await _deposits.GetAllAsync()).FirstOrDefault(x=>x.Status!="VOID"&&x.ReceiptIds.Contains(receipt.Id));
            if(dep!=null)
            {
                vm.Status="SETTLED";
                vm.Events.Add(new(){Date=dep.DepositDate,Title="Masuk setoran",Detail=dep.DepositNo});
            }
        }
        return View(vm);
    }
}

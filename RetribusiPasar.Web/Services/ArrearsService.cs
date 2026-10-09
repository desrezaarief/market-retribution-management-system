using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Models.ViewModels;

namespace RetribusiPasar.Web.Services;

public class ArrearsService
{
    private readonly IRepository<Market> _markets;
    private readonly IRepository<RetributionUnit> _units;
    private readonly IRepository<RetributionType> _types;
    private readonly IRepository<Collector> _collectors;
    private readonly IRepository<Tariff> _tariffs;
    private readonly IRepository<Receipt> _receipts;

    public ArrearsService(IRepository<Market> markets, IRepository<RetributionUnit> units,
        IRepository<RetributionType> types, IRepository<Collector> collectors,
        IRepository<Tariff> tariffs, IRepository<Receipt> receipts)
    {
        _markets=markets; _units=units; _types=types; _collectors=collectors; _tariffs=tariffs; _receipts=receipts;
    }

    public async Task<ArrearsViewModel> BuildAsync(string period, int? marketId=null)
    {
        var endMonth = DateTime.Parse(period + "-01");
        var markets=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);
        var types=(await _types.GetAllAsync()).ToDictionary(x=>x.Id);
        var collectors=(await _collectors.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);
        var tariffs=await _tariffs.GetAllAsync();
        var receipts=(await _receipts.GetAllAsync()).Where(x=>x.Status=="ACTIVE").ToList();
        var vm=new ArrearsViewModel{Period=period,MarketId=marketId};

        foreach(var u in (await _units.GetAllAsync()).Where(x=>x.IsActive && x.Status=="Berfungsi" && (!marketId.HasValue||x.MarketId==marketId)))
        {
            if(!types.TryGetValue(u.RetributionTypeId,out var type) || type.BillingCycle!="Bulanan") continue;
            var start=new DateTime(u.EffectiveFrom.Year,u.EffectiveFrom.Month,1);
            if(start>endMonth) continue;

            var paid=new HashSet<string>();
            foreach(var r in receipts.Where(x=>x.UnitId==u.Id))
            {
                var p=new DateTime(r.PeriodStart.Year,r.PeriodStart.Month,1);
                for(var i=0;i<Math.Max(1,r.Months);i++) paid.Add(p.AddMonths(i).ToString("yyyy-MM"));
            }

            var months=0;
            for(var p=start;p<=endMonth;p=p.AddMonths(1))
                if(!paid.Contains(p.ToString("yyyy-MM"))) months++;

            if(months==0) continue;

            var tariff=tariffs.Where(x=>x.MarketId==u.MarketId&&x.RetributionTypeId==u.RetributionTypeId&&x.EffectiveFrom<=endMonth&&(!x.EffectiveTo.HasValue||x.EffectiveTo.Value>=endMonth))
                              .OrderByDescending(x=>x.EffectiveFrom).FirstOrDefault()?.Amount??0;
            vm.Rows.Add(new ArrearsRow{
                MarketName=markets.GetValueOrDefault(u.MarketId,"-"),
                UnitCode=u.Code,PayerName=u.PayerName,
                CollectorName=u.CollectorId.HasValue?collectors.GetValueOrDefault(u.CollectorId.Value,"-"):"-",
                Months=months,Tariff=tariff
            });
        }
        return vm;
    }
}

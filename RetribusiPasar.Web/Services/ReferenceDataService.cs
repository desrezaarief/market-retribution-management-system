using Microsoft.AspNetCore.Mvc.Rendering;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Services;

public class ReferenceDataService
{
    private readonly IRepository<Market> _markets;
    private readonly IRepository<RetributionType> _types;
    private readonly IRepository<Collector> _collectors;
    private readonly IRepository<RetributionUnit> _units;

    public ReferenceDataService(
        IRepository<Market> markets,
        IRepository<RetributionType> types,
        IRepository<Collector> collectors,
        IRepository<RetributionUnit> units)
    {
        _markets = markets; _types = types; _collectors = collectors; _units = units;
    }

    public async Task<IEnumerable<SelectListItem>> MarketsAsync(int? selected = null)
        => (await _markets.GetAllAsync()).Where(x=>x.IsActive).OrderBy(x=>x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == selected));

    public async Task<IEnumerable<SelectListItem>> TypesAsync(int? selected = null)
        => (await _types.GetAllAsync()).Where(x=>x.IsActive).OrderBy(x=>x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == selected));

    public async Task<IEnumerable<SelectListItem>> CollectorsAsync(int? selected = null, int? marketId = null)
        => (await _collectors.GetAllAsync()).Where(x=>x.IsActive && (!marketId.HasValue || x.MarketId==marketId)).OrderBy(x=>x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == selected));

    public async Task<IEnumerable<SelectListItem>> UnitsAsync(int? selected = null, int? marketId = null)
    {
        var units = (await _units.GetAllAsync())
            .Where(x => x.IsActive && (!marketId.HasValue || x.MarketId == marketId))
            .OrderBy(x => x.MarketId)
            .ThenBy(x => x.Code)
            .ToList();

        var markets = (await _markets.GetAllAsync()).ToDictionary(x => x.Id, x => x.Name);
        var types = (await _types.GetAllAsync()).ToDictionary(x => x.Id, x => x.Name);

        return units.Select(x => new SelectListItem(
            $"{markets.GetValueOrDefault(x.MarketId, "-")} · {x.Code} · {types.GetValueOrDefault(x.RetributionTypeId, "-")} · {x.PayerName}",
            x.Id.ToString(),
            x.Id == selected));
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;

namespace RetribusiPasar.Web.Controllers;

[Authorize(Roles="Administrator,Bendahara")]
public class TariffsController : Controller
{
    private readonly IRepository<Tariff> _repository; private readonly ReferenceDataService _ref; private readonly AuditService _audit;
    private readonly IRepository<Market> _markets; private readonly IRepository<RetributionType> _types;
    public TariffsController(IRepository<Tariff> repository,ReferenceDataService @ref,AuditService audit,IRepository<Market> markets,IRepository<RetributionType> types){_repository=repository;_ref=@ref;_audit=audit;_markets=markets;_types=types;}
    private async Task Setup(){ViewBag.Markets=await _ref.MarketsAsync();ViewBag.Types=await _ref.TypesAsync();}
    public async Task<IActionResult> Index(){ViewBag.MarketNames=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);ViewBag.TypeNames=(await _types.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);return View((await _repository.GetAllAsync()).OrderByDescending(x=>x.EffectiveFrom).ToList());}
    public async Task<IActionResult> Create(){await Setup();return View(new Tariff());}
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Create(Tariff model){
        if(model.EffectiveTo.HasValue&&model.EffectiveTo.Value<model.EffectiveFrom)ModelState.AddModelError(nameof(model.EffectiveTo),"Tanggal akhir tidak boleh sebelum tanggal mulai.");
        var all=await _repository.GetAllAsync();if(all.Any(x=>x.MarketId==model.MarketId&&x.RetributionTypeId==model.RetributionTypeId&&RangesOverlap(x.EffectiveFrom,x.EffectiveTo,model.EffectiveFrom,model.EffectiveTo)))ModelState.AddModelError("", "Periode tarif bertumpuk dengan tarif yang sudah ada.");
        if(!ModelState.IsValid){await Setup();return View(model);}await _repository.AddAsync(model);await _audit.LogAsync("CREATE",nameof(Tariff),model.Id,$"Tarif {model.Amount:N0}");TempData["Success"]="Tarif berhasil disimpan.";return RedirectToAction(nameof(Index));}
    public async Task<IActionResult> Edit(int id){var m=await _repository.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
    [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Edit(int id,Tariff model){
        if(id!=model.Id)return BadRequest();if(model.EffectiveTo.HasValue&&model.EffectiveTo.Value<model.EffectiveFrom)ModelState.AddModelError(nameof(model.EffectiveTo),"Tanggal akhir tidak boleh sebelum tanggal mulai.");
        var all=await _repository.GetAllAsync();if(all.Any(x=>x.Id!=id&&x.MarketId==model.MarketId&&x.RetributionTypeId==model.RetributionTypeId&&RangesOverlap(x.EffectiveFrom,x.EffectiveTo,model.EffectiveFrom,model.EffectiveTo)))ModelState.AddModelError("", "Periode tarif bertumpuk dengan tarif yang sudah ada.");
        if(!ModelState.IsValid){await Setup();return View(model);}await _repository.UpdateAsync(model);await _audit.LogAsync("UPDATE",nameof(Tariff),model.Id,$"Tarif {model.Amount:N0}");TempData["Success"]="Tarif berhasil diperbarui.";return RedirectToAction(nameof(Index));}
    public async Task<IActionResult> Delete(int id){var m=await _repository.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
    [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult> DeleteConfirmed(int id){await _repository.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(Tariff),id,"Tarif dihapus.");TempData["Success"]="Tarif berhasil dihapus.";return RedirectToAction(nameof(Index));}
    private static bool RangesOverlap(DateTime aStart,DateTime? aEnd,DateTime bStart,DateTime? bEnd){var ae=aEnd??DateTime.MaxValue;var be=bEnd??DateTime.MaxValue;return aStart<=be&&bStart<=ae;}
}

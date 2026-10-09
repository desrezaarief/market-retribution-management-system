using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;

namespace RetribusiPasar.Web.Controllers;

[Authorize(Roles="Administrator,Bendahara")]
public class MarketsController : Controller
{
    private readonly IRepository<Market> _repository;
    private readonly AuditService _audit;
    public MarketsController(IRepository<Market> repository, AuditService audit) { _repository=repository; _audit=audit; }

    public async Task<IActionResult> Index() => View((await _repository.GetAllAsync()).OrderBy(x=>x.Name).ToList());
    public IActionResult Create() => View(new Market());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Market model)
    {
        if ((await _repository.GetAllAsync()).Any(x=>x.Code.Equals(model.Code,StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(model.Code),"Kode pasar sudah digunakan.");
        if(!ModelState.IsValid) return View(model);
        await _repository.AddAsync(model); await _audit.LogAsync("CREATE",nameof(Market),model.Id,$"{model.Code} - {model.Name}");
        TempData["Success"]="Pasar berhasil disimpan."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) { var m=await _repository.GetByIdAsync(id); return m==null?NotFound():View(m); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Market model)
    {
        if(id!=model.Id) return BadRequest();
        if ((await _repository.GetAllAsync()).Any(x=>x.Id!=id && x.Code.Equals(model.Code,StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(model.Code),"Kode pasar sudah digunakan.");
        if(!ModelState.IsValid) return View(model);
        await _repository.UpdateAsync(model); await _audit.LogAsync("UPDATE",nameof(Market),model.Id,$"{model.Code} - {model.Name}");
        TempData["Success"]="Pasar berhasil diperbarui."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id){var m=await _repository.GetByIdAsync(id);return m==null?NotFound():View(m);}
    [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id){await _repository.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(Market),id,"Master pasar dihapus.");TempData["Success"]="Pasar berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

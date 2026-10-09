using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;

namespace RetribusiPasar.Web.Controllers;

[Authorize(Roles="Administrator,Bendahara")]
public class RetributionTypesController : Controller
{
    private readonly IRepository<RetributionType> _repository; private readonly AuditService _audit;
    public RetributionTypesController(IRepository<RetributionType> repository, AuditService audit){_repository=repository;_audit=audit;}
    public async Task<IActionResult> Index()=>View((await _repository.GetAllAsync()).OrderBy(x=>x.Name).ToList());
    public IActionResult Create()=>View(new RetributionType());
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(RetributionType model){
        if((await _repository.GetAllAsync()).Any(x=>x.Code.Equals(model.Code,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(model.Code),"Kode sudah digunakan.");
        if(!ModelState.IsValid)return View(model);await _repository.AddAsync(model);await _audit.LogAsync("CREATE",nameof(RetributionType),model.Id,model.Name);TempData["Success"]="Jenis retribusi berhasil disimpan.";return RedirectToAction(nameof(Index));}
    public async Task<IActionResult> Edit(int id){var m=await _repository.GetByIdAsync(id);return m==null?NotFound():View(m);}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Edit(int id,RetributionType model){
        if(id!=model.Id)return BadRequest();if((await _repository.GetAllAsync()).Any(x=>x.Id!=id&&x.Code.Equals(model.Code,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(model.Code),"Kode sudah digunakan.");
        if(!ModelState.IsValid)return View(model);await _repository.UpdateAsync(model);await _audit.LogAsync("UPDATE",nameof(RetributionType),model.Id,model.Name);TempData["Success"]="Jenis retribusi berhasil diperbarui.";return RedirectToAction(nameof(Index));}
    public async Task<IActionResult> Delete(int id){var m=await _repository.GetByIdAsync(id);return m==null?NotFound():View(m);}
    [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult> DeleteConfirmed(int id){await _repository.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(RetributionType),id,"Jenis retribusi dihapus.");TempData["Success"]="Data berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;

namespace RetribusiPasar.Web.Controllers;
[Authorize(Roles="Administrator,Bendahara")]
public class CollectorsController:Controller{
 private readonly IRepository<Collector> _repo;private readonly IRepository<Market> _markets;private readonly ReferenceDataService _ref;private readonly AuditService _audit;
 public CollectorsController(IRepository<Collector> repo,IRepository<Market> markets,ReferenceDataService @ref,AuditService audit){_repo=repo;_markets=markets;_ref=@ref;_audit=audit;}
 private async Task Setup()=>ViewBag.Markets=await _ref.MarketsAsync();
 public async Task<IActionResult> Index(){ViewBag.MarketNames=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);return View((await _repo.GetAllAsync()).OrderBy(x=>x.Name).ToList());}
 public async Task<IActionResult>Create(){await Setup();return View(new Collector());}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(Collector m){if((await _repo.GetAllAsync()).Any(x=>x.Code.Equals(m.Code,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(m.Code),"Kode petugas sudah digunakan.");if(!ModelState.IsValid){await Setup();return View(m);}await _repo.AddAsync(m);await _audit.LogAsync("CREATE",nameof(Collector),m.Id,m.Name);TempData["Success"]="Petugas berhasil disimpan.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Edit(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Edit(int id,Collector m){if(id!=m.Id)return BadRequest();if((await _repo.GetAllAsync()).Any(x=>x.Id!=id&&x.Code.Equals(m.Code,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(m.Code),"Kode petugas sudah digunakan.");if(!ModelState.IsValid){await Setup();return View(m);}await _repo.UpdateAsync(m);await _audit.LogAsync("UPDATE",nameof(Collector),m.Id,m.Name);TempData["Success"]="Petugas berhasil diperbarui.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Delete(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult>DeleteConfirmed(int id){await _repo.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(Collector),id,"Petugas dihapus.");TempData["Success"]="Petugas berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

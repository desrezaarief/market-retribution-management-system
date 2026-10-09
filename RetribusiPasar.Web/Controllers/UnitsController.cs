using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
[Authorize(Roles="Administrator,Bendahara")]
public class UnitsController:Controller{
 private readonly IRepository<RetributionUnit> _repo;private readonly IRepository<Market> _markets;private readonly IRepository<RetributionType> _types;private readonly IRepository<Collector> _collectors;private readonly ReferenceDataService _ref;private readonly AuditService _audit;
 public UnitsController(IRepository<RetributionUnit> repo,IRepository<Market> markets,IRepository<RetributionType> types,IRepository<Collector> collectors,ReferenceDataService @ref,AuditService audit){_repo=repo;_markets=markets;_types=types;_collectors=collectors;_ref=@ref;_audit=audit;}
 private async Task Setup(){ViewBag.Markets=await _ref.MarketsAsync();ViewBag.Types=await _ref.TypesAsync();ViewBag.Collectors=await _ref.CollectorsAsync();}
 public async Task<IActionResult>Index(){ViewBag.MarketNames=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);ViewBag.TypeNames=(await _types.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);ViewBag.CollectorNames=(await _collectors.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);return View((await _repo.GetAllAsync()).OrderBy(x=>x.Code).ToList());}
 public async Task<IActionResult>Create(){await Setup();return View(new RetributionUnit());}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(RetributionUnit m){if((await _repo.GetAllAsync()).Any(x=>x.MarketId==m.MarketId&&x.Code.Equals(m.Code,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(m.Code),"Kode unit sudah ada pada pasar tersebut.");if(!ModelState.IsValid){await Setup();return View(m);}await _repo.AddAsync(m);await _audit.LogAsync("CREATE",nameof(RetributionUnit),m.Id,m.Code);TempData["Success"]="Unit berhasil disimpan.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Edit(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Edit(int id,RetributionUnit m){if(id!=m.Id)return BadRequest();if((await _repo.GetAllAsync()).Any(x=>x.Id!=id&&x.MarketId==m.MarketId&&x.Code.Equals(m.Code,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(m.Code),"Kode unit sudah ada pada pasar tersebut.");if(!ModelState.IsValid){await Setup();return View(m);}await _repo.UpdateAsync(m);await _audit.LogAsync("UPDATE",nameof(RetributionUnit),m.Id,m.Code);TempData["Success"]="Unit berhasil diperbarui.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Delete(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult>DeleteConfirmed(int id){await _repo.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(RetributionUnit),id,"Unit dihapus.");TempData["Success"]="Unit berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

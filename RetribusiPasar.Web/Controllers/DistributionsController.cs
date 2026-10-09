using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
[Authorize(Roles="Administrator,Bendahara")]
public class DistributionsController:Controller{
 private readonly IRepository<MediaDistribution> _repo;private readonly IRepository<Market> _markets;private readonly ReferenceDataService _ref;private readonly RetributionBusinessService _biz;private readonly AuditService _audit;
 public DistributionsController(IRepository<MediaDistribution> repo,IRepository<Market> markets,ReferenceDataService @ref,RetributionBusinessService biz,AuditService audit){_repo=repo;_markets=markets;_ref=@ref;_biz=biz;_audit=audit;}
 private async Task Setup()=>ViewBag.Markets=await _ref.MarketsAsync();
 public async Task<IActionResult>Index(){ViewBag.MarketNames=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);return View((await _repo.GetAllAsync()).OrderByDescending(x=>x.DistributionDate).ToList());}
 public async Task<IActionResult>Create(){await Setup();return View(new MediaDistribution{DistributionDate=DateTime.Today,DistributionNo=$"BA/{DateTime.Today:yyyy}/AUTO"});}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(MediaDistribution m){var e=await _biz.ValidateDistributionAsync(m);if(e!=null)ModelState.AddModelError("",e);if(!ModelState.IsValid){await Setup();return View(m);}await _repo.AddAsync(m);await _audit.LogAsync("CREATE",nameof(MediaDistribution),m.Id,m.DistributionNo);TempData["Success"]="Distribusi berhasil disimpan.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Edit(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Edit(int id,MediaDistribution m){if(id!=m.Id)return BadRequest();var e=await _biz.ValidateDistributionAsync(m,id);if(e!=null)ModelState.AddModelError("",e);if(!ModelState.IsValid){await Setup();return View(m);}await _repo.UpdateAsync(m);await _audit.LogAsync("UPDATE",nameof(MediaDistribution),m.Id,m.DistributionNo);TempData["Success"]="Distribusi berhasil diperbarui.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Delete(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult>DeleteConfirmed(int id){await _repo.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(MediaDistribution),id,"Distribusi dihapus.");TempData["Success"]="Distribusi berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
[Authorize(Roles="Administrator,Bendahara")]
public class StockBatchesController:Controller{
 private readonly IRepository<StockBatch> _repo;private readonly RetributionBusinessService _biz;private readonly AuditService _audit;
 public StockBatchesController(IRepository<StockBatch> repo,RetributionBusinessService biz,AuditService audit){_repo=repo;_biz=biz;_audit=audit;}
 public async Task<IActionResult>Index()=>View((await _repo.GetAllAsync()).OrderByDescending(x=>x.ReceivedDate).ToList());
 public IActionResult Create()=>View(new StockBatch{ReceivedDate=DateTime.Today});
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(StockBatch m){var e=await _biz.ValidateStockAsync(m);if(e!=null)ModelState.AddModelError("",e);if(!ModelState.IsValid)return View(m);await _repo.AddAsync(m);await _audit.LogAsync("CREATE",nameof(StockBatch),m.Id,$"{m.MediaType} {m.StartSerial:000000}-{m.EndSerial:000000}");TempData["Success"]="Stok media berhasil disimpan.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Edit(int id){var m=await _repo.GetByIdAsync(id);return m==null?NotFound():View(m);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Edit(int id,StockBatch m){if(id!=m.Id)return BadRequest();var e=await _biz.ValidateStockAsync(m,id);if(e!=null)ModelState.AddModelError("",e);if(!ModelState.IsValid)return View(m);await _repo.UpdateAsync(m);await _audit.LogAsync("UPDATE",nameof(StockBatch),m.Id,$"{m.MediaType} {m.StartSerial:000000}-{m.EndSerial:000000}");TempData["Success"]="Stok media berhasil diperbarui.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Delete(int id){var m=await _repo.GetByIdAsync(id);return m==null?NotFound():View(m);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult>DeleteConfirmed(int id){await _repo.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(StockBatch),id,"Stok dihapus.");TempData["Success"]="Stok berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

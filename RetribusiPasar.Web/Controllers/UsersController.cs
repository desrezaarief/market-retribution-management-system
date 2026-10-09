using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;

[Authorize(Roles="Administrator")]
public class UsersController:Controller{
 private readonly IRepository<AppUser> _repo;private readonly IRepository<Market> _markets;private readonly ReferenceDataService _ref;private readonly AuditService _audit;
 public UsersController(IRepository<AppUser> repo,IRepository<Market> markets,ReferenceDataService @ref,AuditService audit){_repo=repo;_markets=markets;_ref=@ref;_audit=audit;}
 private async Task Setup()=>ViewBag.Markets=await _ref.MarketsAsync();
 public async Task<IActionResult>Index(){ViewBag.MarketNames=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);return View((await _repo.GetAllAsync()).OrderBy(x=>x.Username).ToList());}
 public async Task<IActionResult>Create(){await Setup();return View(new AppUser());}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(AppUser m){
   if((await _repo.GetAllAsync()).Any(x=>x.Username.Equals(m.Username,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(m.Username),"Username sudah digunakan.");
   if(string.IsNullOrWhiteSpace(m.NewPassword)||m.NewPassword.Length<6)ModelState.AddModelError(nameof(m.NewPassword),"Password minimal 6 karakter.");
   if(!ModelState.IsValid){await Setup();return View(m);}
   var h=new PasswordHasher<AppUser>();m.PasswordHash=h.HashPassword(m,m.NewPassword!);m.NewPassword=null;
   await _repo.AddAsync(m);await _audit.LogAsync("CREATE",nameof(AppUser),m.Id,m.Username);TempData["Success"]="User berhasil disimpan.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Edit(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();m.NewPassword=null;await Setup();return View(m);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Edit(int id,AppUser m){
   if(id!=m.Id)return BadRequest();var old=await _repo.GetByIdAsync(id);if(old==null)return NotFound();
   if((await _repo.GetAllAsync()).Any(x=>x.Id!=id&&x.Username.Equals(m.Username,StringComparison.OrdinalIgnoreCase)))ModelState.AddModelError(nameof(m.Username),"Username sudah digunakan.");
   if(!string.IsNullOrWhiteSpace(m.NewPassword)&&m.NewPassword.Length<6)ModelState.AddModelError(nameof(m.NewPassword),"Password minimal 6 karakter.");
   if(!ModelState.IsValid){await Setup();return View(m);}
   if(string.IsNullOrWhiteSpace(m.NewPassword))m.PasswordHash=old.PasswordHash;else{var h=new PasswordHasher<AppUser>();m.PasswordHash=h.HashPassword(m,m.NewPassword);}m.NewPassword=null;
   await _repo.UpdateAsync(m);await _audit.LogAsync("UPDATE",nameof(AppUser),m.Id,m.Username);TempData["Success"]="User berhasil diperbarui.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Delete(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();await Setup();return View(m);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult>DeleteConfirmed(int id){await _repo.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(AppUser),id,"User dihapus.");TempData["Success"]="User berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

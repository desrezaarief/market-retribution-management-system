using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
[Authorize(Roles="Administrator,Bendahara")]
public class AccountingPeriodsController:Controller{
 private readonly IRepository<AccountingPeriod> _repo;private readonly AuditService _audit;
 public AccountingPeriodsController(IRepository<AccountingPeriod> repo,AuditService audit){_repo=repo;_audit=audit;}
 public async Task<IActionResult>Index()=>View((await _repo.GetAllAsync()).OrderByDescending(x=>x.Period).ToList());
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(string period){if(string.IsNullOrWhiteSpace(period)){TempData["Error"]="Periode wajib diisi.";return RedirectToAction(nameof(Index));}if((await _repo.GetAllAsync()).Any(x=>x.Period==period)){TempData["Error"]="Periode sudah ada.";return RedirectToAction(nameof(Index));}var m=await _repo.AddAsync(new AccountingPeriod{Period=period,Status="OPEN"});await _audit.LogAsync("CREATE",nameof(AccountingPeriod),m.Id,period);return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Close(int id){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();m.Status="CLOSED";m.ClosedAt=DateTime.Now;m.ClosedBy=User.Identity?.Name??"system";await _repo.UpdateAsync(m);await _audit.LogAsync("CLOSE",nameof(AccountingPeriod),id,m.Period);TempData["Success"]="Periode ditutup.";return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Reopen(int id,string reason){var m=await _repo.GetByIdAsync(id);if(m==null)return NotFound();m.Status="OPEN";m.ReopenReason=string.IsNullOrWhiteSpace(reason)?"Reopen oleh administrator":reason;m.ClosedAt=null;m.ClosedBy=null;await _repo.UpdateAsync(m);await _audit.LogAsync("REOPEN",nameof(AccountingPeriod),id,$"{m.Period}: {m.ReopenReason}");TempData["Success"]="Periode dibuka kembali.";return RedirectToAction(nameof(Index));}
}

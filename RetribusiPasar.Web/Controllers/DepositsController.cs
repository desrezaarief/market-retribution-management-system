using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Models.ViewModels;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
public class DepositsController:Controller{
 private readonly IRepository<Deposit> _repo;private readonly IRepository<Receipt> _receipts;private readonly IRepository<Market> _markets;private readonly ReferenceDataService _ref;private readonly RetributionBusinessService _biz;private readonly AuditService _audit;
 public DepositsController(IRepository<Deposit> repo,IRepository<Receipt> receipts,IRepository<Market> markets,ReferenceDataService @ref,RetributionBusinessService biz,AuditService audit){_repo=repo;_receipts=receipts;_markets=markets;_ref=@ref;_biz=biz;_audit=audit;}
 public async Task<IActionResult>Index(){ViewBag.MarketNames=(await _markets.GetAllAsync()).ToDictionary(x=>x.Id,x=>x.Name);return View((await _repo.GetAllAsync()).OrderByDescending(x=>x.DepositDate).ToList());}
 private async Task<DepositEditViewModel> BuildVm(Deposit d,List<int>? selected=null){
   var used=await _biz.DepositedReceiptIdsAsync(d.Id);var rec=(await _receipts.GetAllAsync()).Where(x=>x.Status=="ACTIVE"&&!used.Contains(x.Id)&&(d.MarketId==0||x.MarketId==d.MarketId)).OrderByDescending(x=>x.ReceiptDate).ToList();
   var ids=selected??d.ReceiptIds;return new DepositEditViewModel{Deposit=d,SelectedReceiptIds=ids.ToList(),Markets=await _ref.MarketsAsync(d.MarketId==0?null:d.MarketId),ReceiptChoices=rec.Select(x=>new ReceiptChoice{Id=x.Id,TransactionNo=x.TransactionNo,ReceiptDate=x.ReceiptDate,Amount=x.ReceivedAmount,Selected=ids.Contains(x.Id)}).ToList()};
 }
 public async Task<IActionResult>Create(){return View(await BuildVm(new Deposit{DepositDate=DateTime.Today,FromDate=DateTime.Today,ToDate=DateTime.Today,DepositNo=$"STS/{DateTime.Today:yyyy}/AUTO",BankName="Bank Daerah - Rekening Penerimaan"}));}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Create(DepositEditViewModel vm){
   vm.Deposit.ReceiptIds=vm.SelectedReceiptIds??new();var all=await _receipts.GetAllAsync();var selected=all.Where(x=>vm.Deposit.ReceiptIds.Contains(x.Id)).ToList();vm.Deposit.Amount=selected.Sum(x=>x.ReceivedAmount);
   ModelState.Remove("Deposit.Amount");
   if(!vm.Deposit.ReceiptIds.Any())ModelState.AddModelError("","Pilih minimal satu transaksi penerimaan.");
   if(selected.Any(x=>x.MarketId!=vm.Deposit.MarketId))ModelState.AddModelError("","Semua penerimaan yang dipilih harus berasal dari pasar yang sama dengan setoran.");
   if(!ModelState.IsValid)return View(await BuildVm(vm.Deposit,vm.Deposit.ReceiptIds));
   await _repo.AddAsync(vm.Deposit);if(vm.Deposit.DepositNo.EndsWith("/AUTO")){vm.Deposit.DepositNo=$"STS/{vm.Deposit.DepositDate:yyyy}/{vm.Deposit.Id:000000}";await _repo.UpdateAsync(vm.Deposit);}await _audit.LogAsync("CREATE",nameof(Deposit),vm.Deposit.Id,vm.Deposit.DepositNo);TempData["Success"]="Setoran berhasil disimpan.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Edit(int id){var d=await _repo.GetByIdAsync(id);return d==null?NotFound():View(await BuildVm(d));}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult>Edit(int id,DepositEditViewModel vm){if(id!=vm.Deposit.Id)return BadRequest();vm.Deposit.ReceiptIds=vm.SelectedReceiptIds??new();var all=await _receipts.GetAllAsync();var selected=all.Where(x=>vm.Deposit.ReceiptIds.Contains(x.Id)).ToList();vm.Deposit.Amount=selected.Sum(x=>x.ReceivedAmount);ModelState.Remove("Deposit.Amount");if(!vm.Deposit.ReceiptIds.Any())ModelState.AddModelError("","Pilih minimal satu transaksi penerimaan.");if(selected.Any(x=>x.MarketId!=vm.Deposit.MarketId))ModelState.AddModelError("","Semua penerimaan yang dipilih harus berasal dari pasar yang sama dengan setoran.");if(!ModelState.IsValid)return View(await BuildVm(vm.Deposit,vm.Deposit.ReceiptIds));await _repo.UpdateAsync(vm.Deposit);await _audit.LogAsync("UPDATE",nameof(Deposit),vm.Deposit.Id,vm.Deposit.DepositNo);TempData["Success"]="Setoran berhasil diperbarui.";return RedirectToAction(nameof(Index));}
 public async Task<IActionResult>Delete(int id){var d=await _repo.GetByIdAsync(id);return d==null?NotFound():View(d);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]public async Task<IActionResult>DeleteConfirmed(int id){await _repo.DeleteAsync(id);await _audit.LogAsync("DELETE",nameof(Deposit),id,"Setoran dihapus.");TempData["Success"]="Setoran berhasil dihapus.";return RedirectToAction(nameof(Index));}
}

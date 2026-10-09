using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;
using RetribusiPasar.Web.Services;

namespace RetribusiPasar.Web.Controllers;

public class ReceiptsController : Controller
{
    private readonly IRepository<Receipt> _repo;
    private readonly IRepository<Market> _markets;
    private readonly IRepository<RetributionType> _types;
    private readonly IRepository<RetributionUnit> _units;
    private readonly ReferenceDataService _ref;
    private readonly RetributionBusinessService _biz;
    private readonly AuditService _audit;

    public ReceiptsController(
        IRepository<Receipt> repo,
        IRepository<Market> markets,
        IRepository<RetributionType> types,
        IRepository<RetributionUnit> units,
        ReferenceDataService @ref,
        RetributionBusinessService biz,
        AuditService audit)
    {
        _repo = repo;
        _markets = markets;
        _types = types;
        _units = units;
        _ref = @ref;
        _biz = biz;
        _audit = audit;
    }

    private async Task Setup()
    {
        ViewBag.Markets = await _ref.MarketsAsync();
        ViewBag.Types = await _ref.TypesAsync();
        ViewBag.Units = await _ref.UnitsAsync();
        ViewBag.Collectors = await _ref.CollectorsAsync();
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.MarketNames = (await _markets.GetAllAsync()).ToDictionary(x => x.Id, x => x.Name);
        ViewBag.TypeNames = (await _types.GetAllAsync()).ToDictionary(x => x.Id, x => x.Name);
        ViewBag.UnitNames = (await _units.GetAllAsync()).ToDictionary(x => x.Id, x => x.Code);

        return View((await _repo.GetAllAsync())
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.Id)
            .ToList());
    }

    public async Task<IActionResult> Create()
    {
        await Setup();

        return View(new Receipt
        {
            ReceiptDate = DateTime.Today,
            PeriodStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            TransactionNo = $"TRX/{DateTime.Today:yyyy}/AUTO",
            Status = "ACTIVE"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Receipt model)
    {
        // Jika unit dipilih, source of truth berasal dari master unit.
        await _biz.ApplyUnitDefaultsAsync(model);
        if (model.UnitId.HasValue)
        {
            ModelState.Remove(nameof(model.MarketId));
            ModelState.Remove(nameof(model.RetributionTypeId));
            ModelState.Remove(nameof(model.CollectorId));
        }

        model.ExpectedAmount = await _biz.CalculateExpectedAmountAsync(model);

        var error = await _biz.ValidateReceiptAsync(model);
        if (error != null)
            ModelState.AddModelError("", error);

        if (!ModelState.IsValid)
        {
            await Setup();
            return View(model);
        }

        await _repo.AddAsync(model);

        if (model.TransactionNo.EndsWith("/AUTO", StringComparison.OrdinalIgnoreCase))
        {
            model.TransactionNo = $"TRX/{model.ReceiptDate:yyyy}/{model.Id:000000}";
            await _repo.UpdateAsync(model);
        }

        await _audit.LogAsync("CREATE", nameof(Receipt), model.Id, model.TransactionNo);
        TempData["Success"] = "Penerimaan berhasil disimpan.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _repo.GetByIdAsync(id);
        if (model == null) return NotFound();

        await Setup();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Receipt model)
    {
        if (id != model.Id) return BadRequest();

        var old = await _repo.GetByIdAsync(id);
        if (old?.Status != "ACTIVE")
        {
            TempData["Error"] = "Transaksi yang sudah dihapus tidak dapat diubah.";
            return RedirectToAction(nameof(Index));
        }

        await _biz.ApplyUnitDefaultsAsync(model);
        if (model.UnitId.HasValue)
        {
            ModelState.Remove(nameof(model.MarketId));
            ModelState.Remove(nameof(model.RetributionTypeId));
            ModelState.Remove(nameof(model.CollectorId));
        }

        model.ExpectedAmount = await _biz.CalculateExpectedAmountAsync(model);

        var error = await _biz.ValidateReceiptAsync(model, id);
        if (error != null)
            ModelState.AddModelError("", error);

        if (!ModelState.IsValid)
        {
            await Setup();
            return View(model);
        }

        await _repo.UpdateAsync(model);
        await _audit.LogAsync("UPDATE", nameof(Receipt), model.Id, model.TransactionNo);

        TempData["Success"] = "Penerimaan berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> FormContext(
        int? unitId,
        int? marketId,
        int? retributionTypeId,
        int? collectorId,
        DateTime? periodStart,
        int months = 1,
        int? startSerial = null,
        int? endSerial = null,
        int receiptId = 0)
    {
        var context = await _biz.BuildReceiptFormContextAsync(
            unitId,
            marketId,
            retributionTypeId,
            collectorId,
            periodStart ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            months,
            startSerial,
            endSerial,
            receiptId);

        if (context == null)
            return Json(new { success = false });

        return Json(new
        {
            success = true,
            context.UnitId,
            context.MarketId,
            context.MarketName,
            context.RetributionTypeId,
            context.RetributionTypeName,
            context.CollectorId,
            context.CollectorName,
            context.MediaType,
            context.Tariff,
            context.ExpectedAmount,
            availableRanges = context.AvailableRanges.Select(x => new
            {
                x.StartSerial,
                x.EndSerial,
                x.TotalSheets,
                x.Label
            })
        });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var model = await _repo.GetByIdAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string? deleteReason)
    {
        var model = await _repo.GetByIdAsync(id);
        if (model == null) return NotFound();

        model.Status = "DELETED";
        model.DeleteReason = string.IsNullOrWhiteSpace(deleteReason)
            ? "Dihapus oleh user"
            : deleteReason;

        await _repo.UpdateAsync(model);
        await _audit.LogAsync("DELETE", nameof(Receipt), model.Id, $"{model.TransactionNo}: {model.DeleteReason}");

        TempData["Success"] = "Penerimaan berhasil dihapus.";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
public class ReportsController:Controller{
 private readonly ReportService _report;private readonly ReferenceDataService _ref;
 public ReportsController(ReportService report,ReferenceDataService @ref){_report=report;_ref=@ref;}
 public async Task<IActionResult>Index(string? period,int? marketId){period??=DateTime.Today.ToString("yyyy-MM");ViewBag.Markets=await _ref.MarketsAsync(marketId);return View(await _report.BuildAsync(period,marketId));}
}

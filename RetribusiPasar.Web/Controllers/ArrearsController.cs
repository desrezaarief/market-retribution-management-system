using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Services;
namespace RetribusiPasar.Web.Controllers;
public class ArrearsController:Controller{
 private readonly ArrearsService _service;private readonly ReferenceDataService _ref;
 public ArrearsController(ArrearsService service,ReferenceDataService @ref){_service=service;_ref=@ref;}
 public async Task<IActionResult>Index(string? period,int? marketId){period??=DateTime.Today.ToString("yyyy-MM");ViewBag.Markets=await _ref.MarketsAsync(marketId);return View(await _service.BuildAsync(period,marketId));}
}

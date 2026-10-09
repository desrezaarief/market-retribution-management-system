using Microsoft.AspNetCore.Mvc;
using RetribusiPasar.Web.Services;

namespace RetribusiPasar.Web.Controllers;

public class HomeController : Controller
{
    private readonly DashboardService _dashboard;
    public HomeController(DashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index(string? period)
    {
        period ??= DateTime.Today.ToString("yyyy-MM");
        ViewBag.Period = period;
        return View(await _dashboard.BuildAsync(period));
    }

    public IActionResult Error() => View();
}

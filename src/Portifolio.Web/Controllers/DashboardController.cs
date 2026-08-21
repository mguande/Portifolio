using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class DashboardController(PortfolioService portfolio) : Controller
{
    [HttpGet("/edit")]
    public async Task<IActionResult> Index()
    {
        var data = await portfolio.GetAsync();
        return View(data);
    }
}

using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

public sealed class HomeController(PortfolioService portfolio) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.PortfolioJson = await portfolio.GetJsonAsync(cancellationToken);
        return View();
    }

    [HttpGet("/Home/Error")]
    public IActionResult Error() => Content("Ocorreu um erro.");
}

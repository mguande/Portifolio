using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Models;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class StudioController(PortfolioService portfolio) : Controller
{
    [HttpGet("/edit/estudio")]
    public async Task<IActionResult> Index()
    {
        var data = await portfolio.GetAsync();
        return View(data.Studio);
    }

    [HttpPost("/edit/estudio")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StudioDto studio)
    {
        await portfolio.SaveStudioAsync(studio);
        TempData["Ok"] = "Estúdio salvo.";
        return Redirect("~/edit/estudio");
    }
}

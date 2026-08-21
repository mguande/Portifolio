using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Models;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class CopyController(PortfolioService portfolio) : Controller
{
    [HttpGet("/edit/textos")]
    public async Task<IActionResult> Index()
    {
        var data = await portfolio.GetAsync();
        ViewBag.Stacks = await portfolio.ListStacksAsync();
        return View(data.Copy);
    }

    [HttpPost("/edit/textos")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteCopyDto copy)
    {
        copy.Stats ??= [];
        await portfolio.SaveCopyAsync(copy);
        TempData["Ok"] = "Textos salvos.";
        return Redirect("~/edit/textos");
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class StacksController(PortfolioService portfolio) : Controller
{
    [HttpGet("/edit/stacks")]
    public async Task<IActionResult> Index() => View(await portfolio.ListStacksAsync());

    [HttpPost("/edit/stacks/salvar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string name)
    {
        try
        {
            await portfolio.SaveStackAsync(id, name);
            TempData["Ok"] = id == 0 ? "Stack cadastrada." : "Stack atualizada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return Redirect("~/edit/stacks");
    }

    [HttpPost("/edit/stacks/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await portfolio.DeleteStackAsync(id);
        TempData["Ok"] = "Stack removida.";
        return Redirect("~/edit/stacks");
    }

    [HttpPost("/edit/stacks/{id:int}/mover")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(int id, int delta)
    {
        await portfolio.MoveStackAsync(id, delta);
        return Redirect("~/edit/stacks");
    }
}

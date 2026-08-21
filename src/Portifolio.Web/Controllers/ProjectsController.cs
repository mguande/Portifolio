using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Models;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class ProjectsController(PortfolioService portfolio) : Controller
{
    [HttpGet("/edit/projetos")]
    public async Task<IActionResult> Index() => View(await portfolio.ListProjectsAsync());

    [HttpGet("/edit/projetos/novo")]
    public async Task<IActionResult> Create()
    {
        await FillLookupsAsync();
        return View("Edit", new ProjectRecord { Year = DateTime.Now.Year.ToString() });
    }

    [HttpGet("/edit/projetos/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await portfolio.GetProjectAsync(id);
        if (project is null)
            return NotFound();
        await FillLookupsAsync();
        return View(project);
    }

    [HttpPost("/edit/projetos/salvar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ProjectRecord project)
    {
        project.Authors = Distinct(project.Authors);
        project.Stack = Distinct(project.Stack);
        var peopleIds = (await portfolio.ListProfilesAsync())
            .Select(p => p.PublicId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToList();
        project.Authorship = peopleIds.Count > 1 && peopleIds.All(id => project.Authors.Contains(id, StringComparer.OrdinalIgnoreCase))
            ? "joint"
            : project.Authors.Count == 1 ? project.Authors[0] : "";
        await portfolio.SaveProjectAsync(project);
        TempData["Ok"] = "Projeto salvo.";
        return Redirect("~/edit/projetos");
    }

    [HttpPost("/edit/projetos/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await portfolio.DeleteProjectAsync(id);
        TempData["Ok"] = "Projeto removido.";
        return Redirect("~/edit/projetos");
    }

    private async Task FillLookupsAsync()
    {
        ViewBag.People = await portfolio.ListProfilesAsync();
        ViewBag.KnownStacks = await portfolio.ListKnownStacksAsync();
    }

    private static List<string> Distinct(List<string>? values) =>
        (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}

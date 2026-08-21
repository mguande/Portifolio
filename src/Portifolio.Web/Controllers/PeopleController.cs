using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Models;
using Portifolio.Web.Services;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class PeopleController(PortfolioService portfolio, IWebHostEnvironment env) : Controller
{
    [HttpGet("/edit/perfis")]
    public async Task<IActionResult> Index() => View(await portfolio.ListProfilesAsync());

    [HttpGet("/edit/perfis/novo")]
    public IActionResult Create() => View("Edit", new Profile { PublicId = "", ShortName = "Novo" });

    [HttpGet("/edit/perfis/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var profile = await portfolio.GetProfileAsync(id);
        return profile is null ? NotFound() : View(profile);
    }

    [HttpPost("/edit/perfis/salvar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Profile profile, IFormFile? photo)
    {
        profile.Skills ??= [];
        profile.Socials ??= [];
        profile.Experience ??= [];
        profile.Education ??= [];
        if (photo is { Length: > 0 })
            profile.Photo = await SavePhotoAsync(photo, profile.PublicId);
        await portfolio.SaveProfileAsync(profile);
        TempData["Ok"] = "Perfil salvo.";
        return Redirect($"~/edit/perfis/{profile.Id}");
    }

    [HttpPost("/edit/perfis/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await portfolio.DeleteProfileAsync(id);
        TempData["Ok"] = "Perfil removido.";
        return Redirect("~/edit/perfis");
    }

    [HttpGet("/edit/perfis/importar")]
    public IActionResult ImportNew()
    {
        ViewBag.Profile = null;
        return View("Import", new Person());
    }

    [HttpPost("/edit/perfis/importar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportNew(IFormFile? pdf)
    {
        ViewBag.Profile = null;
        return await ParsePdfAsync(pdf);
    }

    [HttpPost("/edit/perfis/importar/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromImport(Person imported, string? skillsText)
    {
        BindImported(imported, skillsText);
        var id = await portfolio.CreateFromImportAsync(imported);
        TempData["Ok"] = "Perfil criado a partir do PDF.";
        return Redirect($"~/edit/perfis/{id}");
    }

    [HttpGet("/edit/perfis/{id:int}/importar")]
    public async Task<IActionResult> Import(int id)
    {
        var profile = await portfolio.GetProfileAsync(id);
        if (profile is null)
            return NotFound();
        ViewBag.Profile = profile;
        return View(new Person());
    }

    [HttpPost("/edit/perfis/{id:int}/importar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(int id, IFormFile? pdf)
    {
        var profile = await portfolio.GetProfileAsync(id);
        if (profile is null)
            return NotFound();
        ViewBag.Profile = profile;
        return await ParsePdfAsync(pdf);
    }

    [HttpPost("/edit/perfis/{id:int}/importar/aplicar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyImport(int id, Person imported, string? skillsText)
    {
        BindImported(imported, skillsText);
        await portfolio.ApplyImportAsync(id, imported, keepPhoto: true);
        TempData["Ok"] = "Perfil atualizado com o PDF.";
        return Redirect($"~/edit/perfis/{id}");
    }

    private async Task<IActionResult> ParsePdfAsync(IFormFile? pdf)
    {
        if (pdf is null || pdf.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Selecione o PDF exportado pelo LinkedIn.");
            return View("Import", new Person());
        }

        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pdf");
        await using (var stream = System.IO.File.Create(temp))
            await pdf.CopyToAsync(stream);
        try
        {
            return View("Import", LinkedInCvParser.ParsePdf(temp));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível ler o PDF: " + ex.Message);
            return View("Import", new Person());
        }
        finally
        {
            System.IO.File.Delete(temp);
        }
    }

    private static void BindImported(Person imported, string? skillsText)
    {
        imported.Skills = Split(skillsText);
        imported.Experience ??= [];
        imported.Education ??= [];
        imported.Socials ??= [];
        if (imported.Socials.Count == 0 && !string.IsNullOrWhiteSpace(imported.Linkedin))
            imported.Socials = [new SocialLink { Network = "linkedin", Url = imported.Linkedin }];
    }

    private async Task<string> SavePhotoAsync(IFormFile photo, string publicId)
    {
        var ext = Path.GetExtension(photo.FileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".png";
        var name = $"person-{(string.IsNullOrWhiteSpace(publicId) ? "novo" : publicId)}{ext.ToLowerInvariant()}";
        var dir = Path.Combine(env.WebRootPath, "img");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, name);
        await using var stream = System.IO.File.Create(path);
        await photo.CopyToAsync(stream);
        return $"img/{name}";
    }

    private static List<string> Split(string? text) =>
        (text ?? "").Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}

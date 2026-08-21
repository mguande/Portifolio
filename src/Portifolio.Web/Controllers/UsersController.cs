using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portifolio.Web.Models;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class UsersController(UserManager<ApplicationUser> users) : Controller
{
    [HttpGet("/edit/usuarios")]
    public async Task<IActionResult> Index() =>
        View(await users.Users.OrderBy(u => u.UserName).ToListAsync());

    [HttpPost("/edit/usuarios")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string userName, string email, string password, string? displayName)
    {
        var user = new ApplicationUser
        {
            UserName = userName.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? userName.Trim() : displayName.Trim(),
        };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return Redirect("~/edit/usuarios");
        }

        TempData["Ok"] = "Usuário criado.";
        return Redirect("~/edit/usuarios");
    }

    [HttpPost("/edit/usuarios/{id}/excluir")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (await users.Users.CountAsync() <= 1)
        {
            TempData["Error"] = "Não é possível remover o último usuário.";
            return Redirect("~/edit/usuarios");
        }

        var user = await users.FindByIdAsync(id);
        if (user is not null)
            await users.DeleteAsync(user);
        TempData["Ok"] = "Usuário removido.";
        return Redirect("~/edit/usuarios");
    }
}

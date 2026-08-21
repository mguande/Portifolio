using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portifolio.Web.Models;

namespace Portifolio.Web.Controllers;

[Authorize]
public sealed class AccountController(SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet("/edit/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect("~/edit");
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/edit");
        return View();
    }

    [HttpPost("/edit/login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string userName, string password, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Url.Content("~/edit");
        var result = await signIn.PasswordSignInAsync(userName.Trim(), password, isPersistent: true, lockoutOnFailure: false);
        if (result.Succeeded)
            return RedirectAfterLogin(returnUrl);

        ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
        return View();
    }

    [Authorize]
    [HttpPost("/edit/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return Redirect("~/edit/login");
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return LocalRedirect("~/edit");
        if (returnUrl.StartsWith("/edit", StringComparison.OrdinalIgnoreCase))
            return LocalRedirect("~" + returnUrl);
        return LocalRedirect(returnUrl);
    }
}

using Microsoft.AspNetCore.Identity;

namespace Portifolio.Web.Models;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
}

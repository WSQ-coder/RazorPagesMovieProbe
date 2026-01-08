using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

[Authorize(Roles = "admin,user")]
public class IndexModel : PageModel
{
    public string? Name { get; private set; }
    public string? Role { get; private set; }

    public void OnGet()
    {
        Name = User.FindFirst(ClaimsIdentity.DefaultNameClaimType)?.Value;
        Role = User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value;
    }
}
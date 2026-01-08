using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;


public class LoginModel : PageModel
{
    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        //if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError("", "Email и пароль об€зательны.");
            return Page();
        }

        string email = Email.Trim().ToLower();
        string? role = null;

        if (email.Contains("admin"))
            role = "admin";
        else if (email.Contains("user"))
            role = "user";

        if (role == null)
        {
            // ћожно вернуть ошибку или просто перенаправить на accessdenied
            return RedirectToPage("/AccessDenied");
        }

        var claims = new List<Claim>
        {
            new(ClaimsIdentity.DefaultNameClaimType, email),
            new(ClaimsIdentity.DefaultRoleClaimType, role)
            //new(ClaimTypes.Name, email),
            //new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect("/admintest");
        if (string.IsNullOrEmpty(ReturnUrl) || ReturnUrl == "/")
            return Redirect("/");
        else
            return Redirect(ReturnUrl);
    }
}
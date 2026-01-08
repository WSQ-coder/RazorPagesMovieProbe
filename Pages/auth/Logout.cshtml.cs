// /Pages/Logout.cshtml.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


//namespace RazorPagesAuth.Pages.auth;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        // Временно для отладки:
        System.Diagnostics.Debug.WriteLine("Logout triggered!");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login"); 
    }
}
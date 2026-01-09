using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Chat
{
    public class SelectChatPartnerModel : PageModel
    {

        public string? Name { get; private set; }
        public string? Role { get; private set; }
        public string? Email { get; private set; }
        public int? IdAccount { get; private set; }

        public void OnGet()
        {
            Name = User.FindFirst(ClaimTypes.Name)?.Value;
            Role = User.FindFirst(ClaimTypes.Role)?.Value;
            Email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (int.TryParse(User.FindFirst("idaccount")?.Value, out int id))
            {
                IdAccount = id;
            }

        }
    }
}

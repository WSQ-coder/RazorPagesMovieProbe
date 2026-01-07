using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Pages
{
    public class AuthorizationModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public AuthorizationModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string LoginInput { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginInput) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите имя/email и пароль.";
                return Page();
            }

            // 🔍 Ищем пользователя по AccountName ИЛИ Email
            var user = await _context.Accounts
                .Include(a => a.IdRoleNavigation)
                .FirstOrDefaultAsync(a =>
                    (a.AccountName == LoginInput || a.Email == LoginInput) &&
                    a.Password == Password);

            // 💡 НАЧАЛО: ЛОГИРОВАНИЕ ДЛЯ ОТЛАДКИ
            if (user == null)
            {
                Console.WriteLine($"❌ ПОЛЬЗОВАТЕЛЬ НЕ НАЙДЕН! LoginInput='{LoginInput}', Password='{Password}'");
            }
            else
            {
                string roleName = user.IdRoleNavigation?.RoleName ?? "null";
                Console.WriteLine($"✅ НАЙДЕН ПОЛЬЗОВАТЕЛЬ! Имя='{user.AccountName}', Email='{user.Email}', Роль='{roleName}'");
            }
            // 💡 КОНЕЦ: ЛОГИРОВАНИЕ

            if (user != null && user.IdRoleNavigation != null)
            {
                // Сохраняем данные в сессии
                HttpContext.Session.SetString("UserRole", user.IdRoleNavigation.RoleName);
                HttpContext.Session.SetString("UserName", user.AccountName);

                Console.WriteLine($"➡️ Перенаправление по роли: {user.IdRoleNavigation.RoleName}");

                // Перенаправление по роли
                return user.IdRoleNavigation.RoleName switch
                {
                    "admin" => RedirectToPage("/Admin"),
                    "buyer" => RedirectToPage("/Index"),
                    "seller" => RedirectToPage("/Seller/Index"),
                    _ => RedirectToPage("/Authorization")
                };
            }

            ErrorMessage = "Неверное имя/email или пароль.";
            return Page();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase
{
    public class ChatModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public ChatModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.ProductionPurchase Order { get; set; }

        [BindProperty]
        public string NewMessage { get; set; }

        public Models.Account CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Для тестирования используем ID пользователя = 3 (покупатель) или 2 (продавец)
            var currentUserId = 3; // ID покупателя

            Order = await _context.ProductionPurchases
                .Include(p => p.IdBuyerNavigation)
                .Include(p => p.IdSellerNavigation)
                .FirstOrDefaultAsync(m => m.IdProductionPurchase == id);

            if (Order == null)
            {
                return NotFound();
            }

            CurrentUser = await _context.Accounts.FindAsync(currentUserId);

            // Определяем роль текущего пользователя в этом заказе
            if (Order.IdSeller == currentUserId)
            {
                ViewData["UserRole"] = "seller";
            }
            else if (Order.IdBuyer == currentUserId)
            {
                ViewData["UserRole"] = "buyer";
            }
            else
            {
                return Forbid(); // Пользователь не имеет доступа к этому заказу
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (string.IsNullOrEmpty(NewMessage))
            {
                ModelState.AddModelError("NewMessage", "Сообщение не может быть пустым");
                return await OnGetAsync(id);
            }

            var order = await _context.ProductionPurchases.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Для тестирования используем ID пользователя = 3 (покупатель)
            var currentUserId = 3;

            // Добавляем информацию об отправителе и времени к сообщению
            var timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            var senderRole = order.IdSeller == currentUserId ? "Продавец" : "Покупатель";

            // Форматируем историю переписки
            string messageHistory = order.TextAccounts ?? "";
            if (!string.IsNullOrEmpty(messageHistory))
            {
                messageHistory += "\n\n";
            }
            messageHistory += $"[{timestamp}] {senderRole}:\n{NewMessage}";

            order.TextAccounts = messageHistory;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Ошибка при отправке сообщения: {ex.Message}");
                return await OnGetAsync(id);
            }

            // Очищаем поле ввода и перезагружаем страницу
            NewMessage = "";
            return RedirectToPage(new { id = id });
        }
    }
}
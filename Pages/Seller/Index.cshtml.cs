using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;

namespace RazorPagesMovie.Pages.Seller
{
    public class SellerIndexModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public SellerIndexModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public Models.Account SellerAccount { get; set; }
        public IList<Models.Product> Products { get; set; }
        public IList<Models.ProductionPurchase> CustomOrders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Для тестирования используем ID продавца = 2
            var currentUserId = 2;

            // Загружаем данные продавца
            SellerAccount = await _context.Accounts
                .Include(a => a.IdRoleNavigation)
                .FirstOrDefaultAsync(m => m.IdAccount == currentUserId);

            if (SellerAccount == null)
            {
                // Если продавца с ID=2 нет, попробуем найти любого продавца
                SellerAccount = await _context.Accounts
                    .Include(a => a.IdRoleNavigation)
                    .Where(a => a.IdRoleNavigation.RoleName == "seller")
                    .FirstOrDefaultAsync();

                if (SellerAccount == null)
                {
                    // Если нет продавцов, создаем тестовые данные
                    SellerAccount = new Models.Account
                    {
                        IdAccount = currentUserId,
                        AccountName = "Тестовый продавец",
                        IdRoleNavigation = new Models.Role { RoleName = "seller" }
                    };
                }
                currentUserId = SellerAccount.IdAccount;
            }

            // Загружаем товары продавца
            Products = await _context.Products
                .Where(p => p.IdSeller == currentUserId)
                .ToListAsync();

            // Загружаем индивидуальные заказы для продавца
            CustomOrders = await _context.ProductionPurchases
                .Include(po => po.IdBuyerNavigation)
                .Where(po => po.IdSeller == currentUserId)
                .OrderByDescending(po => po.IdProductionPurchase)
                .ToListAsync();

            return Page();
        }
    }
}
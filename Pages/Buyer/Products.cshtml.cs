using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Seller
{
    // Модель для отображения товара на странице
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsIndividual { get; set; }
    }

    public class ProductsModel : PageModel
    {
        // *** ИСПРАВЛЕНО: Используем правильное имя контекста ArtMarketDbContext ***
        private readonly ArtMarketDbContext _context;

        public ProductsModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public int? CurrentBuyerId { get; set; } // Должен быть получен из аутентификации

        public ProductViewModel? IndividualProduct { get; set; }
        public List<ProductViewModel> CatalogProducts { get; set; } = new List<ProductViewModel>();

        public async Task OnGetAsync()
        {
            // --- 1. Определение ID текущего пользователя (Покупателя) ---
            // Реальная логика получения ID покупателя из аутентификации
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int buyerId))
            {
                CurrentBuyerId = buyerId;
            }
            // Для целей тестирования, если аутентификация не настроена, можно использовать заглушку:
            // CurrentBuyerId = 1; 

            // --- 2. Загрузка товаров из базы данных ---
            // *** ИСПРАВЛЕНО: Используем _context.Products вместо _context.Product ***
            var allProducts = await _context.Products
                .Where(p => p.Status == "available")
                .ToListAsync();

            // --- 3. Разделение товаров ---

            if (CurrentBuyerId.HasValue)
            {
                var individualProd = allProducts
                    .FirstOrDefault(p => p.IdIndivBuyer == CurrentBuyerId.Value);

                if (individualProd != null)
                {
                    IndividualProduct = new ProductViewModel
                    {
                        Id = individualProd.IdProduct,
                        Name = individualProd.Name,
                        Price = individualProd.Price,
                        IsIndividual = true
                    };
                }
            }

            // Товары каталога: IdIndivBuyer должен быть NULL
            CatalogProducts = allProducts
                .Where(p => p.IdIndivBuyer == null)
                .Select(p => new ProductViewModel
                {
                    Id = p.IdProduct,
                    Name = p.Name,
                    Price = p.Price,
                    IsIndividual = false
                })
                .ToList();
        }

        // --- Обработка заказа (POST-запрос при нажатии "Заказать") ---
        public async Task<IActionResult> OnPostOrderAsync(int productId)
        {
            // Здесь должна быть реальная логика получения ID покупателя
            if (!CurrentBuyerId.HasValue)
            {
                // Заглушка, если ID покупателя не получен
                TempData["ErrorMessage"] = "Для оформления заказа необходимо войти в систему или установить ID покупателя.";
                return RedirectToPage();
            }

            // *** ИСПРАВЛЕНО: Используем _context.Products.FindAsync ***
            var productToOrder = await _context.Products.FindAsync(productId);

            if (productToOrder == null)
            {
                TempData["ErrorMessage"] = "Выбранный товар не найден.";
                return RedirectToPage();
            }

            // 1. Создание нового заказа (Purchase)
            var newPurchase = new Models.Purchase
            {
                NumberPurchase = "ORD" + System.DateTime.Now.Ticks.ToString(),
                IdSeller = productToOrder.IdSeller,
                IdBuyer = CurrentBuyerId.Value,
                // Использование заглушек для обязательных полей
                AddressDeparture = "Адрес продавца по умолчанию",
                AddressReceiving = "Адрес покупателя по умолчанию",
                MethodDelivery = "courier",
                CreatedAt = DateOnly.FromDateTime(System.DateTime.Now)
            };

            // *** ИСПРАВЛЕНО: Используем _context.Purchases.Add ***
            _context.Purchases.Add(newPurchase);
            await _context.SaveChangesAsync();

            // 2. Создание позиции заказа (ItemPurchase)
            var newItemPurchase = new Models.ItemPurchase
            {
                IdPurchase = newPurchase.IdPurchase,
                IdProduct = productToOrder.IdProduct,
                QuantityInPurchase = 1,
                PriceInPurchase = productToOrder.Price,
                StatusItem = "pending"
            };

            // *** ИСПРАВЛЕНО: Используем _context.ItemPurchases.Add ***
            _context.ItemPurchases.Add(newItemPurchase);

            // Обновление статуса товара, если это индивидуальный товар
            if (productToOrder.IdIndivBuyer.HasValue)
            {
                productToOrder.Status = "reserved";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Товар '{productToOrder.Name}' успешно добавлен в заказ №{newPurchase.NumberPurchase}.";

            return RedirectToPage();
        }
    }
}
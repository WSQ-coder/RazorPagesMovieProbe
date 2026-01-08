using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages
{
    // Модель для отображения товара на странице
    public class IndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeArt { get; set; }
        public decimal Price { get; set; }
        public int QuantityForSale { get; set; }
        public bool IsIndividual { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public IndexModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        public int? CurrentBuyerId { get; set; }

        public IndexViewModel? IndividualProduct { get; set; }
        public List<IndexViewModel> CatalogProducts { get; set; } = new List<IndexViewModel>();

        public async Task OnGetAsync()
        {
            // Получаем ID текущего пользователя, если он авторизован
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int buyerId))
            {
                CurrentBuyerId = buyerId;
            }

            // Загружаем все доступные товары из базы данных
            var allProducts = await _context.Products
                .Where(p => p.Status == "available")
                .ToListAsync();

            // Для авторизованного пользователя показываем персональные товары
            if (CurrentBuyerId.HasValue)
            {
                var individualProd = allProducts
                    .FirstOrDefault(p => p.IdIndivBuyer == CurrentBuyerId.Value);

                if (individualProd != null)
                {
                    IndividualProduct = new IndexViewModel
                    {
                        Id = individualProd.IdProduct,
                        Name = individualProd.Name,
                        TypeArt = individualProd.TypeArt,
                        Price = individualProd.Price,
                        QuantityForSale = individualProd.QuantityForSale,
                        IsIndividual = true
                    };
                }
            }

            // Показываем каталог товаров всем пользователям (авторизованным и нет)
            CatalogProducts = allProducts
                .Where(p => p.IdIndivBuyer == null) // Общедоступные товары
                .Select(p => new IndexViewModel
                {
                    Id = p.IdProduct,
                    Name = p.Name,
                    TypeArt = p.TypeArt,
                    Price = p.Price,
                    QuantityForSale = p.QuantityForSale,
                    IsIndividual = false
                })
                .ToList();
        }

        // Обработка заказа (требует авторизации)
        public async Task<IActionResult> OnPostOrderAsync(int productId)
        {
            // Проверяем авторизацию пользователя
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Для оформления заказа необходимо войти в систему.";
                return RedirectToPage("/Account/Login");
            }

            // Получаем ID текущего пользователя
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int buyerId))
            {
                TempData["ErrorMessage"] = "Не удалось определить ваш аккаунт. Пожалуйста, войдите в систему снова.";
                return RedirectToPage("/Account/Login");
            }

            CurrentBuyerId = buyerId;

            // Находим товар для заказа
            var productToOrder = await _context.Products.FindAsync(productId);

            if (productToOrder == null)
            {
                TempData["ErrorMessage"] = "Выбранный товар не найден.";
                return RedirectToPage();
            }

            // Проверяем, доступен ли товар для заказа
            if (productToOrder.Status != "available" && !(productToOrder.IdIndivBuyer.HasValue && productToOrder.IdIndivBuyer.Value == CurrentBuyerId.Value))
            {
                TempData["ErrorMessage"] = "Этот товар сейчас недоступен для заказа.";
                return RedirectToPage();
            }

            // Проверяем, достаточно ли товара на складе (для общих товаров)
            if (productToOrder.IdIndivBuyer == null && productToOrder.QuantityForSale < 1)
            {
                TempData["ErrorMessage"] = "К сожалению, этого товара нет в наличии.";
                return RedirectToPage();
            }

            try
            {
                // 1. Создание нового заказа (Purchase)
                var newPurchase = new Models.Purchase
                {
                    NumberPurchase = "ORD" + System.DateTime.Now.Ticks.ToString(),
                    IdSeller = productToOrder.IdSeller,
                    IdBuyer = CurrentBuyerId.Value,
                    // Получаем адреса из профиля пользователя или используем заглушки
                    AddressDeparture = "Адрес продавца будет указан при подтверждении заказа",
                    AddressReceiving = "Ваш адрес доставки будет указан при оформлении заказа",
                    MethodDelivery = "courier",
                    CreatedAt = DateOnly.FromDateTime(System.DateTime.Now)
                };

                _context.Purchases.Add(newPurchase);
                await _context.SaveChangesAsync();

                // 2. Создание позиции заказа (ItemPurchase)
                var newItemPurchase = new Models.ItemPurchase
                {
                    IdPurchase = newPurchase.IdPurchase,
                    IdProduct = productToOrder.IdProduct,
                    QuantityInPurchase = 1,
                    PriceInPurchase = productToOrder.Price,
                    StatusItem = "pending" // Ожидает подтверждения продавцом
                };

                _context.ItemPurchases.Add(newItemPurchase);

                // 3. Обновление статуса товара
                if (productToOrder.IdIndivBuyer.HasValue)
                {
                    // Для индивидуальных товаров
                    productToOrder.Status = "reserved";
                }
                else
                {
                    // Для общих товаров уменьшаем количество
                    productToOrder.QuantityForSale -= 1;
                    if (productToOrder.QuantityForSale == 0)
                    {
                        productToOrder.Status = "reserved";
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Товар '{productToOrder.Name}' успешно добавлен в заказ №{newPurchase.NumberPurchase}. Продавец получит уведомление и скоро свяжется с вами для подтверждения заказа.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Произошла ошибка при создании заказа: {ex.Message}. Пожалуйста, попробуйте еще раз.";
            }

            return RedirectToPage();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorPagesMovie.Pages.Order
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ArtMarketDbContext _context;

        public CreateModel(ArtMarketDbContext context)
        {
            _context = context;
        }

        // УБРАЛ [BindProperty] - это было причиной ошибки
        public Models.Product Product { get; set; }

        [BindProperty]
        public int ProductId { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        [Range(1, 100, ErrorMessage = "Количество должно быть от 1 до 100")]
        public int Quantity { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "Способ доставки обязателен")]
        [RegularExpression("^(courier|pickup|mail)$", ErrorMessage = "Неверный способ доставки")]
        public string MethodDelivery { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Адрес получения обязателен")]
        [StringLength(200, ErrorMessage = "Адрес не должен превышать 200 символов")]
        public string AddressReceiving { get; set; }

        [BindProperty]
        public string Comment { get; set; }

        public decimal TotalPrice => Price * Quantity;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Authorization");
            }

            var userEmail = userIdClaim.Value;
            var buyer = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == userEmail);
            if (buyer == null)
            {
                return RedirectToPage("/Authorization");
            }

            Product = await _context.Products
                .Include(p => p.IdSellerNavigation)
                .FirstOrDefaultAsync(m => m.IdProduct == id);

            if (Product == null)
            {
                return NotFound();
            }

            // Проверяем доступность
            if (Product.Status != "available" &&
                !(Product.IdIndivBuyer.HasValue && Product.IdIndivBuyer.Value == buyer.IdAccount))
            {
                TempData["ErrorMessage"] = "Этот товар недоступен для заказа.";
                return RedirectToPage("/Index");
            }

            if (Product.IdIndivBuyer == null && Product.QuantityForSale < 1)
            {
                TempData["ErrorMessage"] = "Товара нет в наличии.";
                return RedirectToPage("/Index");
            }

            ProductId = Product.IdProduct;
            Price = Product.Price;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"=== НАЧАЛО OnPostAsync ===");
            Console.WriteLine($"ProductId: {ProductId}, Quantity: {Quantity}");

            // КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ: Удаляем валидацию для полей Product
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.StartsWith("Product."))
                {
                    ModelState.Remove(key);
                    Console.WriteLine($"Удалена валидация поля: {key}");
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ModelState НЕ валиден ===");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Any())
                    {
                        Console.WriteLine($"Ошибка в поле {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                    }
                }

                await ReloadProductData();
                return Page();
            }

            Console.WriteLine("=== ModelState валиден, продолжаем ===");

            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Authorization");
            }

            var userEmail = userIdClaim.Value;
            var buyer = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == userEmail);
            if (buyer == null)
            {
                return RedirectToPage("/Authorization");
            }

            Console.WriteLine($"ID покупателя: {buyer.IdAccount}");

            Models.Product product = null;
            Models.Purchase newOrder = null;

            try
            {
                // 1. Получаем данные о товаре
                product = await _context.Products
                    .Include(p => p.IdSellerNavigation)
                    .FirstOrDefaultAsync(m => m.IdProduct == ProductId);

                if (product == null)
                {
                    Console.WriteLine("=== Товар не найден ===");
                    ModelState.AddModelError(string.Empty, "Товар не найден.");
                    await ReloadProductData();
                    return Page();
                }

                Console.WriteLine($"Товар найден: {product.Name}, Количество: {product.QuantityForSale}");

                // 2. Проверяем наличие (для неиндивидуальных товаров)
                if (product.IdIndivBuyer == null)
                {
                    Console.WriteLine($"Проверка количества: доступно {product.QuantityForSale}, запрошено {Quantity}");

                    if (product.QuantityForSale < Quantity)
                    {
                        Console.WriteLine("=== Недостаточно товара ===");
                        ModelState.AddModelError(string.Empty,
                            $"? Недостаточно товара в наличии! Доступно: {product.QuantityForSale} шт., Вы запросили: {Quantity} шт.");
                        await ReloadProductData();
                        return Page();
                    }
                }
                else
                {
                    Console.WriteLine($"Индивидуальный товар, покупатель: {product.IdIndivBuyer}");
                    // Для индивидуальных товаров
                    if (product.IdIndivBuyer.Value != buyer.IdAccount)
                    {
                        Console.WriteLine("=== Не тот покупатель для инд.товара ===");
                        ModelState.AddModelError(string.Empty, "? Этот индивидуальный товар предназначен для другого покупателя.");
                        await ReloadProductData();
                        return Page();
                    }
                }

                // 3. ВСЁ ПРОВЕРЕНО - создаём заказ
                Console.WriteLine("=== Создаём заказ ===");
                newOrder = new Models.Purchase
                {
                    NumberPurchase = $"ORD{DateTime.Now:yyyyMMddHHmmssfff}",
                    IdSeller = product.IdSeller,
                    IdBuyer = buyer.IdAccount,
                    AddressDeparture = product.IdSellerNavigation?.Address ?? "Адрес уточняется",
                    AddressReceiving = AddressReceiving,
                    MethodDelivery = MethodDelivery,
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                };

                Console.WriteLine($"Номер заказа: {newOrder.NumberPurchase}");

                _context.Purchases.Add(newOrder);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Заказ создан, ID: {newOrder.IdPurchase}");

                // 4. Пытаемся создать позицию заказа (здесь сработает триггер БД)
                Console.WriteLine("=== Создаём позицию заказа ===");
                var orderItem = new Models.ItemPurchase
                {
                    IdPurchase = newOrder.IdPurchase,
                    IdProduct = product.IdProduct,
                    QuantityInPurchase = Quantity,
                    PriceInPurchase = product.Price,
                    StatusItem = "pending"
                };

                _context.ItemPurchases.Add(orderItem);

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("=== Позиция заказа создана ===");

                    // ВСЁ УСПЕШНО!
                    TempData["SuccessMessage"] = $@"
                        <div class='alert alert-success'>
                            <h5>? Заказ успешно оформлен!</h5>
                            <p><strong>Номер заказа:</strong> {newOrder.NumberPurchase}</p>
                            <p><strong>Товар:</strong> {product.Name}</p>
                            <p><strong>Количество:</strong> {Quantity} шт.</p>
                            <p><strong>Сумма:</strong> {(product.Price * Quantity):C}</p>
                            <p><strong>Статус:</strong> Ожидает подтверждения</p>
                        </div>";

                    Console.WriteLine("=== Перенаправляем на Index ===");
                    return RedirectToPage("/Index");
                }
                catch (DbUpdateException ex)
                when (ex.InnerException?.Message?.Contains("INSUFFICIENT_STOCK") == true ||
                      ex.InnerException?.Message?.Contains("Недостаточно") == true)
                {
                    Console.WriteLine("=== Триггер БД выявил проблему ===");
                    Console.WriteLine($"Ошибка: {ex.InnerException?.Message}");

                    // Триггер БД выявил проблему
                    _context.Purchases.Remove(newOrder);
                    await _context.SaveChangesAsync();

                    ModelState.AddModelError(string.Empty,
                        $"?? Товар закончился! Сейчас доступно: {product.QuantityForSale} шт.");
                    await ReloadProductData();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ОБЩАЯ ОШИБКА: {ex.Message} ===");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Убираем созданный заказ при любой ошибке
                if (newOrder != null && newOrder.IdPurchase > 0)
                {
                    try
                    {
                        var orderToRemove = await _context.Purchases.FindAsync(newOrder.IdPurchase);
                        if (orderToRemove != null)
                        {
                            _context.Purchases.Remove(orderToRemove);
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch { /* Игнорируем ошибку удаления */ }
                }

                ModelState.AddModelError(string.Empty,
                    "? Ошибка при создании заказа. Попробуйте позже.");
                await ReloadProductData();
                return Page();
            }
        }

        private async Task ReloadProductData()
        {
            Console.WriteLine("=== ReloadProductData ===");
            Product = await _context.Products
                .Include(p => p.IdSellerNavigation)
                .FirstOrDefaultAsync(m => m.IdProduct == ProductId);

            if (Product != null)
            {
                Price = Product.Price;
                Console.WriteLine($"Перезагружен товар: {Product.Name}, Цена: {Price}");
            }
            else
            {
                Console.WriteLine("Товар не найден при перезагрузке");
            }
        }
    }
}
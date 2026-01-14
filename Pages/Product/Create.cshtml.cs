using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace RazorPagesMovie.Pages.Product
{
    [Bind(
        "Product.Name",
        "Product.TypeArt",
        "Product.IdSeller",
        "Product.QuantityForSale",
        "Product.Price",
        "Product.Status",
        "Product.IdIndivBuyer"
    )]
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public CreateModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";
            PopulateDropDowns();
            return Page();
        }

        [BindProperty]
        public Models.Product Product { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;
        

        public async Task<IActionResult> OnPostAsync()
        {
            //ReturnUrl = Request.Headers["Referer"].ToString() ?? "/Index";
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }

            // --- ЯВНАЯ УСТАНОВКА NULL ---
            // Если пришло значение 0 (что может случиться, если value="0" или произошла ошибка привязки),
            // мы принудительно ставим null, чтобы в БД записалось пустое поле.
            // Модель Product.IdIndivBuyer является int? (Nullable<int>)
            if (Product.IdIndivBuyer == 0)
            {
                Product.IdIndivBuyer = null;
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage(ReturnUrl);
        }

        private void PopulateDropDowns()
        {
            // 1. Покупатели (IdIndivBuyer)
            var buyers = _context.Accounts
                                 .Include(a => a.IdRoleNavigation)
                                 .Where(a => a.IdRoleNavigation != null && a.IdRoleNavigation.RoleName == "buyer")
                                 .ToList();

            var buyersSelectList = new SelectList(buyers, "IdAccount", "AccountName");
            var items = buyersSelectList.ToList();

            // Добавляем пустой пункт. 
            // Value = "" автоматически превратится в null для поля int?
            items.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "— Выберите покупателя индивидуального заказа (Необязательно) —",
                Selected = true
            });

            ViewData["IdIndivBuyer"] = items;

            // 2. Продавцы (IdSeller)
            var sellers = _context.Accounts
                                  .Include(a => a.IdRoleNavigation)
                                  .Where(a => a.IdRoleNavigation != null && a.IdRoleNavigation.RoleName == "seller")
                                  .ToList();

            ViewData["IdSeller"] = new SelectList(sellers, "IdAccount", "AccountName");
        }
    }
}
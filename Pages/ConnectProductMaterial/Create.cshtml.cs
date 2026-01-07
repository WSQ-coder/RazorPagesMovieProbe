using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial
{
    public class CreateModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public CreateModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // ИСПРАВЛЕНО: Отображаем название товара (Name)
            ViewData["IdProduct"] = new SelectList(_context.Products, "IdProduct", "Name");
            // ИСПРАВЛЕНО: Отображаем название материала (MaterialName)
            ViewData["IdMaterial"] = new SelectList(_context.CatalogForMaterials, "IdMaterial", "MaterialName");
            return Page();
        }

        [BindProperty]
        public Models.ConnectProductMaterial ConnectProductMaterial { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // ИСПРАВЛЕНО: При ошибке валидации нужно повторно заполнить ViewData для списков
                ViewData["IdProduct"] = new SelectList(_context.Products, "IdProduct", "Name");
                ViewData["IdMaterial"] = new SelectList(_context.CatalogForMaterials, "IdMaterial", "MaterialName");
                return Page();
            }

            _context.ConnectProductMaterials.Add(ConnectProductMaterial);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial
{
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public EditModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.ConnectProductMaterial ConnectProductMaterial { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var connectproductmaterial = await _context.ConnectProductMaterials.FirstOrDefaultAsync(m => m.Id == id);
            if (connectproductmaterial == null)
            {
                return NotFound();
            }
            ConnectProductMaterial = connectproductmaterial;

            // ИСПРАВЛЕНО: Отображаем название товара (Name)
            ViewData["IdProduct"] = new SelectList(_context.Products, "IdProduct", "Name");
            // ИСПРАВЛЕНО: Отображаем название материала (MaterialName)
            ViewData["IdMaterial"] = new SelectList(_context.CatalogForMaterials, "IdMaterial", "MaterialName");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
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

            _context.Attach(ConnectProductMaterial).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConnectProductMaterialExists(ConnectProductMaterial.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ConnectProductMaterialExists(int id)
        {
            return _context.ConnectProductMaterials.Any(e => e.Id == id);
        }
    }
}
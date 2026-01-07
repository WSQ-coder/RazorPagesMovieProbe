using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ConnectProductMaterial
{
    public class DetailsModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DetailsModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public Models.ConnectProductMaterial ConnectProductMaterial { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ИСПРАВЛЕНО: Добавляем Include для загрузки названий Товара и Материала
            var connectproductmaterial = await _context.ConnectProductMaterials
                .Include(c => c.IdProductNavigation)
                .Include(c => c.IdMaterialNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (connectproductmaterial is not null)
            {
                ConnectProductMaterial = connectproductmaterial;
                return Page();
            }

            return NotFound();
        }
    }
}
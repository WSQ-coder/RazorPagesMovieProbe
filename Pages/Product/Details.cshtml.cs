using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Product
{
    public class DetailsModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DetailsModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public Models.Product Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.IdSellerNavigation)
                .Include(p => p.IdIndivBuyerNavigation)
                .Include(p => p.ConnectProductMaterials)
                    .ThenInclude(cpm => cpm.IdMaterialNavigation)
                .FirstOrDefaultAsync(m => m.IdProduct == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }
    }
}
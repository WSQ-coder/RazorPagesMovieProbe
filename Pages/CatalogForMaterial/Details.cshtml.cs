using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.CatalogForMaterial
{
    public class DetailsModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DetailsModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public Models.CatalogForMaterial CatalogForMaterial { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catalogformaterial = await _context.CatalogForMaterials.FirstOrDefaultAsync(m => m.IdMaterial == id);

            if (catalogformaterial is not null)
            {
                CatalogForMaterial = catalogformaterial;

                return Page();
            }

            return NotFound();
        }
    }
}

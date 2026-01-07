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
    public class DeleteModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DeleteModel(RazorPagesMovie.Models.ArtMarketDbContext context)
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

            if (connectproductmaterial is not null)
            {
                ConnectProductMaterial = connectproductmaterial;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var connectproductmaterial = await _context.ConnectProductMaterials.FindAsync(id);
            if (connectproductmaterial != null)
            {
                ConnectProductMaterial = connectproductmaterial;
                _context.ConnectProductMaterials.Remove(ConnectProductMaterial);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

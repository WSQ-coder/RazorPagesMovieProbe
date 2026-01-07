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
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public IndexModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public IList<Models.ConnectProductMaterial> ConnectProductMaterial { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ConnectProductMaterial = await _context.ConnectProductMaterials
                .Include(c => c.IdMaterialNavigation)
                .Include(c => c.IdProductNavigation).ToListAsync();
        }
    }
}

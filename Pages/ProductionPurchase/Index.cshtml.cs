using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase
{
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public IndexModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public IList<Models.ProductionPurchase> ProductionPurchase { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ProductionPurchase = await _context.ProductionPurchases
                .Include(p => p.IdBuyerNavigation)
                .Include(p => p.IdProductNavigation)
                .Include(p => p.IdSellerNavigation).ToListAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ItemPurchase
{
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public IndexModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        public IList<Models.ItemPurchase> ItemPurchase { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ItemPurchase = await _context.ItemPurchases
                .Include(i => i.IdProductNavigation)
                .Include(i => i.IdPurchaseNavigation).ToListAsync();
        }
    }
}

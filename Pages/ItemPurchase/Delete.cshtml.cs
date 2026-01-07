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
    public class DeleteModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public DeleteModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.ItemPurchase ItemPurchase { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var itempurchase = await _context.ItemPurchases.FirstOrDefaultAsync(m => m.IdItemPurchase == id);

            if (itempurchase is not null)
            {
                ItemPurchase = itempurchase;

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

            var itempurchase = await _context.ItemPurchases.FindAsync(id);
            if (itempurchase != null)
            {
                ItemPurchase = itempurchase;
                _context.ItemPurchases.Remove(ItemPurchase);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

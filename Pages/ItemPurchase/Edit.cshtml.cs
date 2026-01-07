using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ItemPurchase
{
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public EditModel(RazorPagesMovie.Models.ArtMarketDbContext context)
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

            var itempurchase =  await _context.ItemPurchases.FirstOrDefaultAsync(m => m.IdItemPurchase == id);
            if (itempurchase == null)
            {
                return NotFound();
            }
            ItemPurchase = itempurchase;
           ViewData["IdProduct"] = new SelectList(_context.Products, "IdProduct", "IdProduct");
           ViewData["IdPurchase"] = new SelectList(_context.Purchases, "IdPurchase", "IdPurchase");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ItemPurchase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemPurchaseExists(ItemPurchase.IdItemPurchase))
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

        private bool ItemPurchaseExists(int id)
        {
            return _context.ItemPurchases.Any(e => e.IdItemPurchase == id);
        }
    }
}

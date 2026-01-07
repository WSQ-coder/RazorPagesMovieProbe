using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.ProductionPurchase
{
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Models.ArtMarketDbContext _context;

        public EditModel(RazorPagesMovie.Models.ArtMarketDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.ProductionPurchase ProductionPurchase { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionpurchase =  await _context.ProductionPurchases.FirstOrDefaultAsync(m => m.IdProductionPurchase == id);
            if (productionpurchase == null)
            {
                return NotFound();
            }
            ProductionPurchase = productionpurchase;
           ViewData["IdBuyer"] = new SelectList(_context.Accounts, "IdAccount", "IdAccount");
           ViewData["IdProduct"] = new SelectList(_context.Products, "IdProduct", "IdProduct");
           ViewData["IdSeller"] = new SelectList(_context.Accounts, "IdAccount", "IdAccount");
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

            _context.Attach(ProductionPurchase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductionPurchaseExists(ProductionPurchase.IdProductionPurchase))
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

        private bool ProductionPurchaseExists(int id)
        {
            return _context.ProductionPurchases.Any(e => e.IdProductionPurchase == id);
        }
    }
}

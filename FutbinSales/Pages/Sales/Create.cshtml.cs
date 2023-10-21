using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FutbinSales.Core.Players;
using FutbinSales.Data;

namespace FutbinSales.Pages.Sales
{
    public class CreateModel : PageModel
    {
        private readonly FutbinSales.Data.SalesContext _context;

        public CreateModel(FutbinSales.Data.SalesContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Player Player { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid)
          {
              return Page();
          }
          _context.Players.Add(Player);
          await _context.SaveChangesAsync();

          return RedirectToPage("./Index");
        }
    }
}

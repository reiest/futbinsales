using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FutbinSales.Core.Players;
using FutbinSales.Core.Players.Services;
using FutbinSales.Data;

namespace FutbinSales.Pages.dummy
{
    public class CreateModel : PageModel
    {
        private readonly FutbinSales.Data.SalesContext _context;
        private readonly ISalesService _salesService;

        public CreateModel(FutbinSales.Data.SalesContext context, ISalesService salesService)
        {
            _context = context;
            _salesService = salesService;
        }

        [BindProperty] public bool getData { get; set; } = true;
        [BindProperty]
        public DateTime lastSaleDate { get; set; }
        
        public IActionResult OnGet(int id)
        {
        ViewData["PlayerId"] = new SelectList(_context.Players, "Id", "Id");
        if (_context.Sales.Where(s => s.PlayerId == id).Any())
            {
                lastSaleDate = _context.Sales.Where(s => s.PlayerId == id).OrderByDescending(s => s.SaleDate).FirstOrDefault().SaleDate;
            }
            
            if (lastSaleDate > DateTime.Now.AddHours(-3))
            {
                getData = false;
            }
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid || _context.Sales == null)
            {
                return Page();
            }
            // If last sale date is more than 4 hours old, scrape data
            if (lastSaleDate < DateTime.Now.AddHours(-4))
            {
                await _salesService.ScrapeDataAsync(id);
            }
            return RedirectToPage("./Index");
        }
    }
}

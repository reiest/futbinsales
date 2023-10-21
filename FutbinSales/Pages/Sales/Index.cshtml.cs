using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FutbinSales.Core.Players;
using FutbinSales.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FutbinSales.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly FutbinSales.Data.SalesContext _context;

        public IndexModel(FutbinSales.Data.SalesContext context)
        {
            _context = context;
        }

        public IList<Player> Player { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Players != null)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
                
                Player = await _context.Players.Include(p => p.Sales).ToListAsync();
            }
        }
    }
}

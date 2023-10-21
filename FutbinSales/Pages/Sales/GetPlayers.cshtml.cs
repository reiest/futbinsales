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

namespace FutbinSales.Pages.Sales
{
    public class GetPlayersModel : PageModel
    {
        private readonly FutbinSales.Data.SalesContext _context;
        private readonly IPlayerService _playerService;

        public GetPlayersModel(FutbinSales.Data.SalesContext context, IPlayerService playerService)
        {
            _context = context;
            _playerService = playerService;
        }

        
        [BindProperty]
        public string url { get; set; }
        [BindProperty]
        public string categoryName { get; set; }
        [BindProperty]
        public bool loading { get; set; } = false;
        
        
        public IActionResult OnGet()
        {
            return Page();
        }
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            loading = true;
            await _playerService.GetPlayers(url, new Category(categoryName));

            return RedirectToPage("./Index");
        }
    }
}

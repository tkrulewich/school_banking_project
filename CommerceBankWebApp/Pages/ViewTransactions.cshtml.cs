using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommerceBankWebApp.Areas.Identity.Data;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommerceBankWebApp.Pages
{
    public class ViewTransactionsModel : PageModel
    {
        public List<Transaction> Transactions { get; set; }

        private readonly ILogger<ViewTransactionsModel> _logger;
        private readonly ApplicationDbContext _context;

        private readonly SignInManager<CommerceBankWebAppUser> _signInManager;
        private readonly UserManager<CommerceBankWebAppUser> _userManager;

        // The constructor enables logging and access to the database
        public ViewTransactionsModel(ILogger<ViewTransactionsModel> logger,
            ApplicationDbContext context,
            SignInManager<CommerceBankWebAppUser> signInManager,
            UserManager<CommerceBankWebAppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;

            Transactions = new List<Transaction>();
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var bankAccountQuery = await _context.BankAccounts.Where(ac => ac.CommerceBankWebAppUser.Id == user.Id).ToListAsync();

            foreach (var result in bankAccountQuery)
            {
                var transactionQuery = await _context.Transactions.Where(t => t.AccountNumber == result.AccountNumber).ToListAsync();

                Transactions.AddRange(transactionQuery);
            }

            return Page();
        }
    }
}
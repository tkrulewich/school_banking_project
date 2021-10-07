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
        public List<BankAccount> BankAccounts { get; set; }

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

            BankAccounts = new List<BankAccount>();
        }


        public async Task<IActionResult> OnGetAsync()
        {
            // read all bank accounts in the databse that are associated with the current user's id
            // Note the Include method is necessary to load the associated list of transactions in each account
            var user = await _userManager.GetUserAsync(User);
            BankAccounts = await _context.BankAccounts.Where( b => b.CommerceBankWebAppUserId == user.Id).Include(a => a.Transactions).ToListAsync();

            return Page();
        }
    }
}
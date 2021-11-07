using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace CommerceBankWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public List<BankAccount> BankAccounts { get; set; }
        public BankAccount AccountToDisplay { get; set; }

        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        // The constructor enables logging and access to the database
        public IndexModel(ApplicationDbContext context,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;

            BankAccounts = new List<BankAccount>();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (_signInManager.IsSignedIn(User))
            {
                // if the user is an admin, read ALL bank accounts
                if (User.IsInRole("admin"))
                {
                    BankAccounts = _context.GetAllBankAccountsWithTransactions();
                }
                // otherwise just read the accounts that belong to the user
                else
                {
                    // get users info
                    var userId = _userManager.GetUserId(User);
                    BankAccounts = _context.GetAllBankAccountsFromUserWithTransactions(userId);
                }
            }
            return Page();
        }
    }
}

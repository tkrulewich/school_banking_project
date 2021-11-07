using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public BankAccount AccountToDisplay { get; set; }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // The constructor enables logging and access to the database
        public ViewTransactionsModel(ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            BankAccounts = new List<BankAccount>();
        }

        public async Task<IActionResult> OnGetAsync(int? index)
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

            //TODO: Warn on bad index. Currently invalid index just gives a list of valid accounts with to choose, but no error message

            // if no account index was given, but there is only one account available to user, select that account
            if (!index.HasValue && BankAccounts.Count() == 1) index = 0;

            // if the user specified an account to view, or there is only 1 account available
            if (index.HasValue)
            {
                // if the index that the user gave is in the list of accounts available to the user
                if (index >= 0 && index < BankAccounts.Count)
                {
                    AccountToDisplay = BankAccounts[index.Value];
                    AccountToDisplay.Transactions = AccountToDisplay.Transactions.OrderByDescending(t => t.DateProcessed).ThenByDescending(t => t.Id).ToList(); 
                }

            }

            return Page();
        }
    }
}
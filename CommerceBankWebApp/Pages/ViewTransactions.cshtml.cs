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
        public BankAccount AccountToDisplay { get; set; }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<CommerceBankWebAppUser> _userManager;

        // The constructor enables logging and access to the database
        public ViewTransactionsModel(ApplicationDbContext context,
            UserManager<CommerceBankWebAppUser> userManager)
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
                BankAccounts = await _context.BankAccounts.Include( ac => ac.Transactions).ToListAsync();
            }
            // otherwise just read the accounts that belong to the user
            else
            {
                // read all bank accounts in the databse that are associated with the current user's id
                // Note the Include method is necessary to load the associated list of transactions in each account
                var user = await _userManager.GetUserAsync(User);
                BankAccounts = await _context.BankAccounts.Where(b => b.CommerceBankWebAppUserId == user.Id).Include(a => a.Transactions).ToListAsync();
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
                }

            }

            return Page();
        }
    }
}
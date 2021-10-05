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

namespace CommerceBankWebApp.Areas.Identity.Pages.Account.Manage
{
    public class BankAccountsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CommerceBankWebAppUser> _userManager;
        public List<BankAccount> BankAccounts { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public BankAccountsModel(ApplicationDbContext context, UserManager<CommerceBankWebAppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task ReadAccounts()
        {
            var user = await _userManager.GetUserAsync(User);

            BankAccounts = await _context.BankAccounts.Where(b => b.CommerceBankWebAppUserId == user.Id).ToListAsync();

        }

        public class InputModel
        {
            public long AccountNumber { get; set; }
            public string AccountType { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await ReadAccounts();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            long accountNumber = long.Parse(Request.Form["AccountNumber"]);
            string accountType = Request.Form["AccountType"];

            if ((await _context.BankAccounts.Where(b => b.AccountNumber == accountNumber).ToListAsync()).Count() != 0)
            {

                return await OnGetAsync();
            } else
            {
                var user = await _userManager.GetUserAsync(User);

                BankAccount account = new BankAccount()
                {
                    AccountNumber = accountNumber,
                    AccountType = accountType,
                    CommerceBankWebAppUserId = user.Id
                };

                _context.BankAccounts.Attach(account);

                await _context.SaveChangesAsync();
            }

            return await OnGetAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        public string[] AccountTypes = new[] { "Checking", "Savings" };

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
            [Required(ErrorMessage = "Account number is required!")]
            public long AccountNumber { get; set; }
            [Required(ErrorMessage = "Account type is required!")]
            public string AccountType { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await ReadAccounts();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await ReadAccounts();

            if (!ModelState.IsValid) return Page();

            long accountNumber = Input.AccountNumber;
            string accountType = Input.AccountType;

            if ((await _context.BankAccounts.Where(b => b.AccountNumber == accountNumber).ToListAsync()).Count() != 0)
            {

                return Page();
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

            await ReadAccounts();
            return Page();
        }
    }
}

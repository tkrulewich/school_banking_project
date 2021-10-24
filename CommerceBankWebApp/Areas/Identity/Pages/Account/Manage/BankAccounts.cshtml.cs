using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly UserManager<IdentityUser> _userManager;
        public List<Models.BankAccount> BankAccounts { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string[] AccountTypes = new[] { "Checking", "Savings" };

        public BankAccountsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Gets BankAccount's associated with the user and stores list in the public property BankAccounts
        private async Task ReadAccounts()
        {
            // get users info
            var userId = _userManager.GetUserId(User);

            BankAccounts = await _context.GetAllBankAccountsFromUser(userId);

        }

        public class InputModel
        {
            [Required(ErrorMessage = "Account number is required!")]
            public string AccountNumber { get; set; }
            [Required(ErrorMessage = "Account type is required!")]
            public string AccountType { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Read users bank accounts from database
            await ReadAccounts();

            return Page();
        }

        // this page Post's when a user tries to add a bank account.
        public async Task<IActionResult> OnPostAsync()
        {
            // Read users bank accounts from database
            await ReadAccounts();

            // if the user didnt input valid data into the form, return to the page, displaying error messages
            if (!ModelState.IsValid) return Page();


            // if there are already bankaccounts in the system with that number, return to the page without doing anything
            // TODO: Show an error message or something!
            if ((await _context.BankAccounts.Where(b => b.AccountNumber == Input.AccountNumber).ToListAsync()).Count() != 0)
            {

                return Page();
            } else
            {
                // get users info
                var userId =  _userManager.GetUserId(User);
                var accountHolder = await _context.AccountHolders.Where(ach => ach.WebAppUserId == userId).SingleOrDefaultAsync();

                BankAccountType accountType;

                if (Input.AccountType == "Checking") accountType = BankAccountType.Checking;
                else accountType = BankAccountType.Savings;


                // create an account using the form data the user supplied
                Models.BankAccount account = new Models.BankAccount()
                {
                    AccountNumber = Input.AccountNumber,
                    BankAccountTypeId = accountType.Id,
                    AccountHolderId = accountHolder.Id
                };

                // add the account to the database and save changes
                _context.BankAccounts.Attach(account);
                await _context.SaveChangesAsync();
            }
            // re-read accounts associated with this user from the database, and display the page
            // If everything went well the new account should be among the accounts read
            await ReadAccounts();
            return Page();
        }
    }
}

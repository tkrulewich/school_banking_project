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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace CommerceBankWebApp.Pages
{
    public class AddTransactionModel : PageModel
    {
        private readonly ILogger<AddTransactionModel> _logger;
        private readonly ApplicationDbContext _context;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        // this is the list of accounts that can be selected in the drop down menu
        public List<SelectListItem> AccountSelectList { get; set; }

        // this is the list of withdraw and deposit for selecting transaction type
        public List<SelectListItem> TransactionTypeSelectList { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public AddTransactionModel(
            ILogger<AddTransactionModel> logger,
            ApplicationDbContext context,
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // The from data will be bound to these properties on submit
        public class InputModel
        {
            [Required]
            [Display(Name = "Account Number")]
            public string AccountNumber { get; set; }

            [Required]
            [Display(Name = "Date Processed")]
            [DataType(DataType.Date)]
            public DateTime ProcessingDate { get; set; }

            [Required]
            [Display(Name = "Deposit or Withdrawal")]
            public bool IsCredit { get; set; }

            [Required]
            [Display(Name = "Amount")]
            [DataType(DataType.Currency)]
            public double Amount { get; set; }

            [Required]
            [Display(Name = "Description")]
            public string Description { get; set; }
        }

        // Reads all accounts associated with user and uses that data to populate the drop-down box, used to select which account to add transaction to
        public async Task ReadAccounts()
        {
            AccountSelectList = new List<SelectListItem>();
            TransactionTypeSelectList = new List<SelectListItem>();

            List<BankAccount> bankAccounts;

            // if the user is an admin, read ALL bank acccounts
            if (User.IsInRole("admin"))
            {
                bankAccounts = await _context.GetAllBankAccounts();
            }
            // otherwise read only the bank accounts associated with the user
            else
            {
                // get all accounts associated with the user
                var userId = _userManager.GetUserId(User);
                bankAccounts = await _context.GetAllBankAccountsFromUser(userId);
            }

            // for each associated account create an option in the drop down menu of format ACCOUNT NUMBER -- ACCOUNT TYPE
            foreach (var account in bankAccounts)
            {
                AccountSelectList.Add(new SelectListItem()
                {
                    Text = $"{account.AccountNumber} -- {account.BankAccountType.Description}",
                    Value = account.AccountNumber.ToString()
                });
            }

            //assign withdraw and deposit to transaction type list
            TransactionTypeSelectList.Add(new SelectListItem()
            {
                Text = "Withdraw",
                Value = "false"
            });
            TransactionTypeSelectList.Add(new SelectListItem()
            {
                Text = "Deposit",
                Value = "true"
            });
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // upon entering the page read associated accounts so we can display the drop down box
            await ReadAccounts();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // when we post, read the accounts
            await ReadAccounts();

            // if the data given isn't valid return to the page and display errors
            if (!ModelState.IsValid) return Page();

            BankAccount bankAccount = await _context.GetBankAccountByAccountNumber(Input.AccountNumber);

            TransactionType transactionType;

            if (Input.IsCredit) transactionType = TransactionType.Deposit;
            else transactionType = TransactionType.Withdrawal;

            // create a new transaction assocated with this bank account
            Transaction transaction = new Transaction()
            {
                BankAccount = bankAccount,
                Amount = Input.Amount,
                DateProcessed = Input.ProcessingDate,
                TransactionTypeId = transactionType.Id,
                Description = Input.Description
            };

            _context.Transactions.Add(transaction);

            if (transactionType == TransactionType.Deposit) bankAccount.Balance += transaction.Amount;
            else bankAccount.Balance -= transaction.Amount;

            _context.BankAccounts.Attach(bankAccount);

            await _context.SaveChangesAsync();

            // redirect to the view transactions page
            return RedirectToPage("/ViewTransactions");

        }
    }
}

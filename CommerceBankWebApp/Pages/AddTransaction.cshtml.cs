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
        private readonly NotificationRuleCheck _ruleChecker;

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
            UserManager<IdentityUser> userManager,
            NotificationRuleCheck ruleChecker)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _ruleChecker = ruleChecker;
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
            public decimal Amount { get; set; }

            [Required]
            [Display(Name = "Description")]
            public string Description { get; set; }
        }

        // Reads all accounts associated with user and uses that data to populate the drop-down box, used to select which account to add transaction to
        public async void ReadAccounts()
        {
            AccountSelectList = new List<SelectListItem>();
            TransactionTypeSelectList = new List<SelectListItem>();

            List<BankAccount> bankAccounts;

            // if the user is an admin, read ALL bank acccounts
            if (User.IsInRole("admin"))
            {
                bankAccounts = _context.GetAllBankAccounts();
            }
            // otherwise read only the bank accounts associated with the user
            else
            {
                // get all accounts associated with the user
                var userId = _userManager.GetUserId(User);
                bankAccounts = _context.GetAllBankAccountsFromUser(userId);
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

        public IActionResult OnGetAsync()
        {
            // upon entering the page read associated accounts so we can display the drop down box
            ReadAccounts();
            //get notification rules from user
            var userId = _userManager.GetUserId(User);
            _ruleChecker.rules = _context.GetNotificationRulesFromUser(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // when we post, read the accounts
            ReadAccounts();

            // if the data given isn't valid return to the page and display errors
            if (!ModelState.IsValid) return Page();

            BankAccount bankAccount = _context.GetBankAccountByAccountNumber(Input.AccountNumber);

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

            var notifs = _ruleChecker.Check(transaction);
            foreach(Notification notif in notifs)
            {
                notif.BankAccount = bankAccount;
                notif.BankAccountId = bankAccount.Id;
                notif.DateProcessed = Input.ProcessingDate;

                _context.AddNotification(notif);
            }

            _context.BankAccounts.Attach(bankAccount);

            await _context.SaveChangesAsync();

            // redirect to the view transactions page
            return RedirectToPage("/ViewTransactions");

        }
    }
}

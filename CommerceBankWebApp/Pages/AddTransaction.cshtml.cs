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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace CommerceBankWebApp.Pages
{
    public class AddTransactionModel : PageModel
    {
        public List<Transaction> Transactions { get; set; }

        private readonly ILogger<AddTransactionModel> _logger;
        private readonly ApplicationDbContext _context;

        private readonly SignInManager<CommerceBankWebAppUser> _signInManager;
        private readonly UserManager<CommerceBankWebAppUser> _userManager;

        public List<SelectListItem> accountList { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public AddTransactionModel(
            ILogger<AddTransactionModel> logger,
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

        public class InputModel
        {
            [Required]
            [Display(Name = "Account Number")]
            public long AccountNumber { get; set; }

            [Required]
            [Display(Name = "Date Processed")]
            [DataType(DataType.Date)]
            public DateTime ProcessingDate { get; set; }

            [Required]
            [Display(Name = "Credit")]
            public bool IsCredit { get; set; }

            [Required]
            [Display(Name = "Amount")]
            [DataType(DataType.Currency)]
            public double Amount { get; set; }

            [Required]
            [Display(Name = "Description")]
            public string Description { get; set; }
        }

        public async Task ReadAccounts()
        {
            accountList = new List<SelectListItem>();

            var user = await _userManager.GetUserAsync(User);

            var accounts = await _context.BankAccounts.Where(ac => ac.CommerceBankWebAppUser.Id == user.Id).ToListAsync();

            foreach (var account in accounts)
            {
                accountList.Add(new SelectListItem()
                {
                    Text = $"{account.AccountNumber} -- {account.AccountType}",
                    Value = account.AccountNumber.ToString()
                });
            }
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

            var bankAccountQuery = await _context.BankAccounts.Where(ac => ac.AccountNumber == Input.AccountNumber).ToListAsync();
            BankAccount bankAccount = bankAccountQuery.First();
            

            Transaction t = new Transaction()
            {
                BankAccount = bankAccount,
                Amount = Input.Amount,
                ProcessingDate = Input.ProcessingDate,
                IsCredit = Input.IsCredit,
                Description = Input.Description
            };

            _context.Transactions.Add(t);

            await _context.SaveChangesAsync();

            return RedirectToPage("/ViewTransactions");

        }
    }
}

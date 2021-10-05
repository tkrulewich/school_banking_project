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
            [Display(Name = "Account Number")]
            public long AccountNumber { get; set; }
            [Display(Name = "Date Processed")]
            [DataType(DataType.Date)]
            public DateTime ProcessingDate { get; set; }
            [Display(Name = "Credit")]
            public bool IsCredit { get; set; }
            [Display(Name = "Amount")]
            public double Amount { get; set; }
            [Display(Name = "Description")]
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            accountList = new List<SelectListItem>();

            var user = await _userManager.GetUserAsync(User);

            var accounts = await _context.BankAccounts.Where(ac => ac.CommerceBankWebAppUser.Id == user.Id).ToListAsync();

            foreach (var account in accounts)
            {
                accountList.Add(new SelectListItem()
                {
                    Text = account.AccountNumber.ToString(),
                    Value = account.AccountNumber.ToString()
                });
            }


            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            long accountNumber = long.Parse(Request.Form["accountNumber"]);
            
            var bankAccountQuery = await _context.BankAccounts.Where(ac => ac.AccountNumber == accountNumber).ToListAsync();
            BankAccount bankAccount = bankAccountQuery.First();

            string amount = Request.Form["amount"];
            string date = Request.Form["date"];
            string isCredit = Request.Form["isCredit"];

            if (String.IsNullOrEmpty(isCredit)) isCredit = "false";
            
            string description = Request.Form["description"];

            Transaction t = new Transaction()
            {
                BankAccount = bankAccount,
                Amount = Double.Parse(amount),
                ProcessingDate = DateTime.Parse(date),
                IsCredit = Boolean.Parse(isCredit),
                Description = description
            };

            _context.Transactions.Add(t);

            await _context.SaveChangesAsync();

            return RedirectToPage("/ViewTransactions");

        }
    }
}

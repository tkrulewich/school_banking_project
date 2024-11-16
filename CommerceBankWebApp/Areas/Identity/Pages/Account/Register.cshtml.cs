using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadsheetLight;

namespace CommerceBankWebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        // private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            // IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            // _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // All the required input fields for the register form are here 
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Account Number")]
            public string AccountNumber { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true

                };

                var accountHolder = await _context.AccountHolders.Where(ach =>
                       (ach.FirstName == Input.FirstName && ach.LastName == Input.LastName)
                       || ach.EmailAddress == Input.Email
                       || ach.PhoneNumber == Input.PhoneNumber)
                    .Include(ach => ach.BankAccounts)
                    .Include(ach => ach.BankAccounts)
                    .FirstOrDefaultAsync();

                if (accountHolder == null)
                {
                    accountHolder = new AccountHolder()
                    {
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        DateOfBirth = Input.DateOfBirth,
                        EmailAddress = Input.Email,
                        PhoneNumber = Input.PhoneNumber,
                        DateBecameCustomer = DateTime.Today,

                        BankAccounts = new List<BankAccount>(),
                    };
                }
                else
                {
                    // if there is an account holder with matching information, but one one more of the fields is incorrect
                    if (accountHolder.FirstName != Input.FirstName
                        || accountHolder.LastName != accountHolder.LastName
                        || accountHolder.DateOfBirth != Input.DateOfBirth
                        || accountHolder.EmailAddress != Input.Email
                        || accountHolder.PhoneNumber != Input.PhoneNumber)
                    {
                        return Content("Could not create account! Please verify the information!", "text/html");
                    }
                    if (!String.IsNullOrEmpty(accountHolder.WebAppUserId))
                    {
                        return Content("User already has an account!", "text/html");
                    }


                }

                // try to find a bank account in the database with matching account number
                BankAccount bankAccount = _context.GetBankAccountByAccountNumber(Input.AccountNumber);

                // if there is an existing account matching that account number
                if (bankAccount != null)
                {
                    // if the bank account is already associated with another user account, leave the page and give an error message
                    // TODO: This should be be prettier
                    if (bankAccount.AccountHolderId != null)
                    {
                        return Content("Bank account already registered. Do you already have an account?", "text/html");
                    }

                }
                else
                {
                    // if there was no matching account make one
                    bankAccount = (new Models.BankAccount
                    {
                        AccountNumber = Input.AccountNumber,
                        BankAccountTypeId = BankAccountType.Checking.Id,
                        AccountHolderId = accountHolder.Id,
                        DateAccountOpened = DateTime.Today
                    });
                }

                // Now create the user. This was the default template code from this point on
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _context.BankAccounts.Attach(bankAccount);
                    accountHolder.BankAccounts.Add(bankAccount);

                    accountHolder.WebAppUserId = user.Id;

                    _context.AccountHolders.Attach(accountHolder);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await CommerceBankWebApp.Models.Email.SendConfirmationEmail(accountHolder, callbackUrl);

                    // try to find a bank account in the database with matching account number
                    bankAccount = _context.GetBankAccountByAccountNumberWithTransactions(Input.AccountNumber);

                    await ReadExcelData("/app/transactions.xlsx", bankAccount);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task ReadExcelData(string filePath, BankAccount bankAccount) {
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                return;
            } else
            {
                _logger.LogInformation($"File found: {filePath}");
            }

            using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            // create a spread sheet light document using the file provided
            SLDocument document = new SLDocument(file);

            // get a list of all work sheets in the excel file
            var workSheets = document.GetSheetNames();

            // for each sheet in the document i.e. "Cust A", "Cust B"
            // TODO: Crashes on invalid files
            foreach (var sheet in workSheets)
            {
                // select the sheet
                document.SelectWorksheet(sheet);

                // get statistics about the document i.e. number of rows, cols, etc.
                SLWorksheetStatistics stats = document.GetWorksheetStatistics();

                // for each row (1 indexed. We start with row 2 because the first row is the headings)
                for (short row = 2; row <= stats.EndRowIndex; row++)
                {
                    // data we intend to read from the curent row

                    BankAccountType accountType = BankAccountType.Checking; // default to Checking, but if data is provided in the excel sheet, can be changed.
                    string accountNumber = null;
                    DateTime? dateProcessed = null;
                    decimal? balance = null;
                    TransactionType transactionType = null;
                    decimal? amount = null;
                    string description = null;
                    string location = null;

                    // for every column in the current row
                    for (short col = 1; col <= 8; col++)
                    {
                        // if there is no value, skip it
                        if (!document.HasCellValue(row, col)) continue;

                        // Find out what the column header is (first row column j)
                        // and use that column header to figure out what to do with data in the current cell
                        // TODO: HANDLE BAD DATA
                        switch (document.GetCellValueAsString(1, col))
                        {
                            case "Account Type":
                                string accountTypeStr = document.GetCellValueAsString(row, col);
                                if (accountTypeStr == "Checking") accountType = BankAccountType.Checking;
                                else if (accountTypeStr == "Savings") accountType = BankAccountType.Savings;
                                break;
                            case "Acct #":
                                accountNumber = document.GetCellValueAsString(row, col);
                                break;
                            case "Processing Date":
                                dateProcessed = document.GetCellValueAsDateTime(row, col);
                                break;
                            case "Balance":
                                balance = Convert.ToDecimal(document.GetCellValueAsDouble(row, col));
                                break;
                            case "CR (Deposit) or DR (Withdrawal":
                            case "CR (Deposit) or DR (Withdrawal)":
                                string cellText = document.GetCellValueAsString(row, col);

                                if (cellText == "Deposit" || cellText == "CR") transactionType = TransactionType.Deposit;
                                else transactionType = TransactionType.Withdrawal;
                                break;
                            case "Amount":
                                amount = Convert.ToDecimal(document.GetCellValueAsDouble(row, col));
                                break;
                            case "Description 1":
                                description = document.GetCellValueAsString(row, col);
                                break;
                            case "Location":
                                location = document.GetCellValueAsString(row, col);
                                break;
                        }
                    }

                    if (accountNumber != "211111110") continue;
                    accountNumber = "211111110";

                    // if the data contains an account number and a date, we got something to work with
                    if (!String.IsNullOrEmpty(accountNumber) && dateProcessed.HasValue)
                    {
                        // if the transaction has an ammount and we know whether its a credit or withdrawal (we already know its got an acct# and date)
                        if (amount.HasValue && transactionType != null)
                        {
                            // if the account type doesnt match the account type of the existing account log a warning
                            if (accountType.Id != bankAccount.BankAccountTypeId)
                            {
                                _logger.LogWarning($"Account type wrong. Account exists already with type: {accountType.Description} but excel data claims type {bankAccount.BankAccountType.Description}");
                            }

                            if (String.IsNullOrEmpty(description)) description = "No description";

                            // create a new transaction using data
                            // (some of this data may have been modified above if this was the first transaction under this acct#)
                            Transaction transaction = new Transaction()
                            {
                                DateProcessed = dateProcessed.Value,
                                TransactionTypeId = transactionType.Id,
                                Amount = amount.Value,
                                Description = description,
                                Location = location
                            };

                            // Add the transaction to the database
                            bankAccount.Transactions.Add(transaction);
                            if (transactionType == TransactionType.Deposit) bankAccount.Balance += transaction.Amount;
                            else bankAccount.Balance -= transaction.Amount;
                        }
                    }
                }

                _context.BankAccounts.Attach(bankAccount);
                await _context.SaveChangesAsync();
            }

        }
    }
}

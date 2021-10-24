using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace CommerceBankWebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
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
                var user = new IdentityUser {
                    UserName = Input.Email,
                    Email = Input.Email
                    

                };

                // TODO: THIS IS NOT A GOOD WAY OF VERIFYING IDENTITY
                var accountHolder = await _context.AccountHolders.Where(ach => ach.DateOfBirth == Input.DateOfBirth && ach.FirstName == Input.FirstName).Include(ach => ach.BankAccounts)
                    .Include( ach => ach.BankAccounts)
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
                } else
                {
                    // if the user alreadu has a web app account (different than a record from the bank)
                    if (!String.IsNullOrEmpty(accountHolder.WebAppUserId))
                    {
                        return Content("User already has an account!", "text/html");
                    }
                }

                // try to find a bank account in the database with matching account number
                BankAccount bankAccount = await _context.GetBankAccountByAccountNumber(Input.AccountNumber);

                // if there is an existing account matching that account number
                if (bankAccount != null)
                {
                    // if the bank account is already associated with another user account, leave the page and give an error message
                    // TODO: This should be be prettier
                    if (bankAccount.AccountHolderId != null)
                    {
                        return Content("Bank account already registered. Do you already have an account?", "text/html");
                    }

                } else
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

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CommerceBankWebApp.Areas.Identity.Data;
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
        private readonly SignInManager<CommerceBankWebAppUser> _signInManager;
        private readonly UserManager<CommerceBankWebAppUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<CommerceBankWebAppUser> userManager,
            SignInManager<CommerceBankWebAppUser> signInManager,
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
            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            public DateTime DOB { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Account Number")]
            public long AccountNumber { get; set; }
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
                var user = new CommerceBankWebAppUser {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Name = Input.Name,
                    DOB = Input.DOB,
                    PhoneNumber = Input.PhoneNumber
                };

                // query all bank accounts where the account number matches the one the user supplied
                var query = await _context.BankAccounts.Where( ac => ac.AccountNumber == Input.AccountNumber).ToListAsync();

                // wil store the bank account we intend to asociate with the user account here
                BankAccount bankAccount = new BankAccount();

                // if there is an existing account matching that account number
                if (query.Count() != 0)
                {
                    // set account to the first account in our query (there can only be one because the AccountNumber column is unique)
                    bankAccount = query.First();

                    // if the bank account is already associated with another user account, leave the page and give an error message
                    // TODO: This should be be prettier
                    if (!String.IsNullOrEmpty(bankAccount.CommerceBankWebAppUserId))
                    {
                        return Content("Bank account already registered. Do you already have an account?", "text/html");
                    }
                } else
                {
                    // if there was no matching account make one
                    bankAccount = (new Models.BankAccount
                    {
                        AccountNumber = Input.AccountNumber,
                        AccountType = "Checking"
                    });


                    // add that account to the database and save changes
                    await _context.BankAccounts.AddAsync(bankAccount);
                    await _context.SaveChangesAsync();
                }

                // Add the account to the list of accounts associated with the user.
                // EF Core framework will take care of the database foreign keys for us with some magic
                user.BankAccounts = new List<Models.BankAccount>() { bankAccount };

                // Now create the user. This was the default template code from this point on
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
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

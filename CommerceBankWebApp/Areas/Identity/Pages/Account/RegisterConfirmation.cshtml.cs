﻿using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Linq;

namespace CommerceBankWebApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        // private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            // _sender = sender;
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("~/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
                var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? Request.Host.ToString();
                var baseUrl = $"{scheme}://{host}{Request.PathBase}";

                EmailConfirmationUrl = $"{baseUrl}/Identity/Account/ConfirmEmail?userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}&returnUrl={Uri.EscapeDataString(returnUrl)}";

                // EmailConfirmationUrl = $"{Request.Scheme}://{Request.Host}" + Url.Page(
                //     "/Account/ConfirmEmail",
                //     pageHandler: null,
                //     values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                //     protocol: null); // Don't specify the protocol here, as we're handling it manually


            }

            Console.WriteLine("EmailConfirmationUrl: " + EmailConfirmationUrl);

            return Page();
        }
    }
}

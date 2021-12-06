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

namespace CommerceBankWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class AddNotificationRuleModel : PageModel
    {
        public List<Rule> Rules { get; set; }
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AddNotificationRuleModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList TypeOptions { get; set; }
        public class InputModel
        {
            [Display(Name = "Rule Type")]
            public char Type { get; set; }

            [Required]
            [Display(Name = "Threshold")]
            public decimal Threshold { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;
            TypeOptions = new SelectList(
                             new List<SelectListItem>
                             {
                                new SelectListItem { Text = "Threshold", Value = "t"},
                                new SelectListItem { Text = "Negative Balance", Value = "n"},
                             }, "Value", "Text");

            Input = new InputModel()
            {
                Type = 't'
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // if the user is an admin, read ALL notifications
            if (User.IsInRole("admin"))
            {
                Notifications = _context.GetAllNotificationRules();
            }
            // otherwise just read the notifications that belong to the user
            else
            {
                // get users info
                var userId = _userManager.GetUserId(User);
                Notifications = _context.GetAllNotificationRulesFromUser(userId);
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            AccountHolder accountHolder = await _context.AccountHolders.Where(ach => ach.WebAppUserId == user.Id).FirstOrDefaultAsync();
            var message = "";
            switch (Input.Type)
            {
                case 't':
                    message = ("Transaction over $" + Input.Threshold);
                    break;
                case 'n':
                    message = ("Negative account balance!");
                    break;

            }

            var newRule = new Rule()
            {
                accountHolder = accountHolder,
                Type = Input.Type,
                Threshold = Input.Threshold,
                Message = message
            };

            _context.AddNotificationRule(newRule);

            StatusMessage = "Your notification rules have been updated";
            return RedirectToPage();
        }
    }
}

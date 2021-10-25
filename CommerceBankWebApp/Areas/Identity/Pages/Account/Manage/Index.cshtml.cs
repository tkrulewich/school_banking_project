using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommerceBankWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CommerceBankWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
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

        // Most of this code is default, but Full name and date of birth have been added
        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [DataType(DataType.Text)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Birth Date")]
            public DateTime DateOfBirth { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            var accountHolder = await _context.AccountHolders.Where(ach => ach.WebAppUserId == user.Id).FirstOrDefaultAsync();

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = accountHolder.FirstName,
                LastName = accountHolder.LastName,
                DateOfBirth = accountHolder.DateOfBirth
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var accountHolder = await _context.AccountHolders.Where(ach => ach.WebAppUserId == user.Id).FirstOrDefaultAsync();


            if (Input.FirstName != accountHolder.FirstName)
            {
                accountHolder.FirstName = Input.FirstName;
            }

            if (Input.LastName != accountHolder.LastName)
            {
                accountHolder.LastName = Input.LastName;
            }

            if (Input.DateOfBirth != accountHolder.DateOfBirth)
            {
                accountHolder.DateOfBirth = Input.DateOfBirth;
            }


            await _userManager.UpdateAsync(user);
            _context.AccountHolders.Attach(accountHolder);

            await _signInManager.RefreshSignInAsync(user);

            await _context.SaveChangesAsync();

            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}

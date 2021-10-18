using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommerceBankWebApp.Areas.Identity.Data;
using CommerceBankWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CommerceBankWebApp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<CommerceBankWebAppUser> _userManager;
        private readonly SignInManager<CommerceBankWebAppUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<CommerceBankWebAppUser> userManager,
            SignInManager<CommerceBankWebAppUser> signInManager,
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
            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Birth Date")]
            public DateTime DOB { get; set; }
        }

        private async Task LoadAsync(CommerceBankWebAppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            var accountHolder = await _context.AccountHolders.Where(ach => ach.CommerceBankWebAppUserId == user.Id).FirstOrDefaultAsync();

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Name = accountHolder.Name,
                DOB = accountHolder.DOB
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

            var accountHolder = await _context.AccountHolders.Where(ach => ach.CommerceBankWebAppUserId == user.Id).FirstOrDefaultAsync();


            if (Input.Name != accountHolder.Name)
            {
                accountHolder.Name = Input.Name;
            }

            if (Input.DOB != accountHolder.DOB)
            {
                accountHolder.DOB = Input.DOB;
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

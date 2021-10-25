using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CommerceBankWebApp.Pages.Admin
{
    public class AddAccountHolderModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddAccountHolderModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (_context.AccountHolders.Where( ach => 
                    (ach.FirstName == Input.FirstName && ach.LastName == Input.LastName)
                    || ach.EmailAddress == Input.EmailAddress
                    || ach.PhoneNumber == Input.PhoneNumber).Any())
                {
                    return Content("There is already an existing account holder with matching name, email, or phone number", "text/html");
                }

                AccountHolder accountHolder = new AccountHolder()
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    DateOfBirth = Input.DateOfBirth,
                    PhoneNumber = Input.PhoneNumber,
                    EmailAddress = Input.EmailAddress,
                    DateBecameCustomer = Input.DateBecameCustomer
                };

                await _context.AccountHolders.AddAsync(accountHolder);
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            [Required]
            [Display(Name = "Birth Date")]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }
            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
            [Required]
            [Display(Name = "Email Address")]
            public string EmailAddress { get; set; }
            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Date Became Customer")]
            public DateTime DateBecameCustomer { get; set; } = DateTime.Now;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommerceBankWebApp.Areas.Identity.Pages.Account
{
    public class NotificationsModel : PageModel
    {

        public List<Notification> Notifications { get; set; }

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // The constructor enables logging and access to the database
        public NotificationsModel(ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            Notifications = new List<Notification>();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            // if the user is an admin, read ALL notifications
            if (User.IsInRole("admin"))
            {
                Notifications = _context.GetAllNotifications();
            }
            // otherwise just read the notifications that belong to the user
            else
            {
                // get users info
                var userId = _userManager.GetUserId(User);
                 Notifications = _context.GetAllNotificationsFromUser(userId);
            }

            return Page();
        }
    }
}

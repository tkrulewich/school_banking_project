using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Areas.Identity.Data
{
    public class CommerceBankWebAppUser : IdentityUser
    {
        public String Name { get; set; }
        public DateTime DOB { get; set; }

        // The Bank Accounts associated with the user
        public virtual List<BankAccount> BankAccounts { get; set; }
    }
}

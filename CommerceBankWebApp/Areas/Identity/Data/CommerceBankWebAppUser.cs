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
        [PersonalData]
        public String Name { get; set; }

        [PersonalData]
        public DateTime DOB { get; set; }

        [InverseProperty("CommerceBankWebAppUser")]
        public virtual List<BankAccount> BankAccounts { get; set; }
    }
}

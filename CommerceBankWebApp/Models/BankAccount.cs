using CommerceBankWebApp.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    [Table("BankAccounts")]
    public class BankAccount
    {
        [Required]
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("AccountNumber")]
        public long AccountNumber { get; set; }

        [Column("AccountType")]
        [Required]
        public string AccountType { get; set; }

        
        public string CommerceBankWebAppUserId { get; set; }
        [ForeignKey("CommerceBankWebAppUserId")]
        public virtual CommerceBankWebAppUser CommerceBankWebAppUser { get; set; }

        [InverseProperty("BankAccount")]
        public List<Transaction> Transactions { get; set; }
    }
}

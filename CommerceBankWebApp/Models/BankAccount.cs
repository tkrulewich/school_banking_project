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

        // This ID is the primary key of the transaction. This is not the account number
        // The sql database requires you to change SET_IDENTITY ON if you wish to insert a row and supply the account number
        // Given that we need to insert accounts and provide the account number, this was easier.
        [Required]
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        // The account number, stored as a long integer. There is code requiring this column to be unique in ApplicationDbContext.cs
        [Required]
        [Column("AccountNumber")]
        public long AccountNumber { get; set; }

        // account type is stored as a string. We could change that in the future, but this functions
        [Column("AccountType")]
        [Required]
        public string AccountType { get; set; }

        // This references the user account that owns the bank account.
        // The id is the primary key for the user's row, or we can access an instance of CommerceBankWebAppUser directly
        public string CommerceBankWebAppUserId { get; set; }
        [ForeignKey("CommerceBankWebAppUserId")]
        public virtual CommerceBankWebAppUser CommerceBankWebAppUser { get; set; }

        // A list of all transactions associated with this account
        public List<Transaction> Transactions { get; set; }
    }
}

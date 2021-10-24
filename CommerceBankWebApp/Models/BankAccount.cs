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

        // The account number, not the primary key, there is code requiring this column to be unique in ApplicationDbContext.cs
        [Required]
        [Column("AccountNumber")]
        public string AccountNumber { get; set; }

        public int BankAccountTypeId { get; set; }
        [ForeignKey("BankAccountTypeId")]
        public BankAccountType BankAccountType { get; set; }

        // Balance of Account
        [Column("Balance")]
        [Required]
        public double Balance { get; set; }

        // This references the account holder
        // The id is the primary key for the account holder record, or we can access an instance of the class using AccountHolder
        public int? AccountHolderId { get; set; }
        [ForeignKey("AccountHolderId")]
        public AccountHolder AccountHolder { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateAccountOpened { get; set; }

        // A list of all transactions associated with this account
        public List<Transaction> Transactions { get; set; }
    }
}

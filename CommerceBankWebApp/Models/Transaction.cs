using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{

    [Table("Transactions")]
    public class Transaction
    {
        [Column("Id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        public int BankAccountId{ get; set; }
        [ForeignKey("BankAccountId")]
        public virtual BankAccount BankAccount { get; set; }

        [Column("DateProcessed")]
        [Required]
        public DateTime DateProcessed { get; set; }

        public int TransactionTypeId { get; set; }
        [ForeignKey("TransactionTypeId")]
        public TransactionType TransactionType { get; set; }

        [Column("Amount")]
        [Required]
        public decimal Amount { get; set; }

        [Required]
        [Column("Description")]
        public string Description { get; set; }

        public string Location { get; set; }
    }
}

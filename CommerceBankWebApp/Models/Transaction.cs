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

        [Column("AccountType")]
        [Required]
        public string AccountType { get; set; }

        [Column("AccountNumber")]
        [Required]
        public long AccountNumber { get; set; }

        [Column("ProcesingDate")]
        [Required]
        public DateTime ProcessingDate { get; set; }

        [Column("Balance")]
        public double? Balance { get; set; }

        [Column("IsCredit")]
        [Required]
        public bool IsCredit { get; set; }

        [Column("Amount")]
        [Required]
        public double Amount { get; set; }

        [Required]
        [Column("Description")]
        public string Description { get; set; }
    }
}

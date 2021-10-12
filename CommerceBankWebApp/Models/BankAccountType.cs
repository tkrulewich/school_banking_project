using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    public class BankAccountType
    {
        [Column("Id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int Id { get; set; }

        public string Description { get; set; }

        public static BankAccountType Checking = new BankAccountType()
        {
            Id = 0,
            Description = "Checking"
        };

        public static BankAccountType Savings = new BankAccountType()
        {
            Id = 1,
            Description = "Savings"
        };

        public static bool operator ==(BankAccountType rhs, BankAccountType lhs)
        {
            if (rhs is null)
            {
                if (lhs is null) return true;
                else return false;
            }

            if (lhs is null) return false;

            return rhs.Id == lhs.Id;
        }

        public static bool operator !=(BankAccountType rhs, BankAccountType lhs)
        {
            return !(rhs == lhs);
        }
    }
}

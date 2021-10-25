using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    public class TransactionType
    {
        [Column("Id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int Id { get; set; }

        public string Description { get; set; }

        static public TransactionType Withdrawal = new TransactionType()
        {
            Id = 0,
            Description = "Withdrawal"
        };

        static public TransactionType Credit = new TransactionType()
        {
            Id = 1,
            Description = "Credit"
        };

        public static bool operator ==(TransactionType rhs, TransactionType lhs)
        {
            if (rhs is null)
            {
                if (lhs is null) return true;
                else return false;
            }

            if (lhs is null) return false;

            return rhs.Id == lhs.Id;
        }

        public static bool operator !=(TransactionType rhs, TransactionType lhs)
        {
            return !(rhs == lhs);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    [Table("AccountHolders")]
    public class AccountHolder
    {
        [Required]
        [Key]
        [Column("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public String FirstName { get; set; }
        public String LastName { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string EmailAddress { get; set; }

        public DateTime DateBecameCustomer { get; set; }

        [InverseProperty("AccountHolder")]
        public virtual List<BankAccount> BankAccounts { get; set; }


        [Column("WebAppUserId")]
        public string WebAppUserId { get; set; }
    }
}

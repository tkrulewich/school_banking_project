using CommerceBankWebApp.Areas.Identity.Data;
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

        [Column("Name")]
        public String Name { get; set; }

        [Column("DOB")]
        [DataType(DataType.DateTime)]
        public DateTime DOB { get; set; }

        [InverseProperty("AccountHolder")]
        public virtual List<BankAccount> BankAccounts { get; set; }

        [Column("CommerceBankWebAppUserId")]
        public string CommerceBankWebAppUserId { get; set; }
    }
}

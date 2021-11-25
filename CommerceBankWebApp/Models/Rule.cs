using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    [Table("NotificationRules")]
    public class Rule
    {
        [Column("Id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        public int AccountHolderId { get; set; }
        [ForeignKey("AccountHolderId")]
        public virtual AccountHolder accountHolder { get; set; }
        [Column("Type")]
        [Required]
        public char Type { get; set; }

        [Column("Threshold")]
        public decimal threshold { get; set; }
        [Column("Location")]
        public string location { get; set; }
        [Column("Vendor")]
        public string vendor { get; set; }
        [Column("Category")]
        public string category { get; set; }

        [Column("Message")]
        public string message { get; set; }
    }
}

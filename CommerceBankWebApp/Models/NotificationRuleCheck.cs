using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    
    //used in service to check a transaction against multiple notification rules
    public class NotificationRuleCheck
    {
        public List<NotificationRule> rules;

        public NotificationRuleCheck()
        {
            rules = new List<NotificationRule>();
            rules.Add(new ThresholdRule(1000));
        }

        public List<Notification> Check(Transaction transaction)
        {
            var notifs = new List<Notification>();
            foreach (NotificationRule rule in rules)
            {
                var notif = rule.ApplyRule(transaction);
                if (notif != null)
                {
                    notifs.Add(notif);
                }
            }
            return notifs;
        }

    }
    //Class to polymorph into individual rule types
    [Table("NotificationRules")]
    [Keyless]
    [NotMapped]
    public class NotificationRule
    {
        public int Id;

        [ForeignKey("AccountHolderID")]
        public AccountHolder accountHolder;

        public double threshold;
        public string location;
        public string vendor;
        public string category;
        public virtual Notification ApplyRule(Transaction transaction)
        {
            Notification notif = null;
            return notif;
        }
    }
    //Rule to send notification if transaction amount is greater than threshold amount 
    public class ThresholdRule : NotificationRule
    {
        public ThresholdRule(double threshold)
        {
            this.threshold = threshold;
        }
        public override Notification ApplyRule(Transaction transaction)
        {
            Notification notif = null;
            if (transaction.Amount > this.threshold)
            {
                notif = new Notification
                {
                    Message = "Withdrawal greater than 1000!!!"
                };
            }
            return notif;
        }
    }
    
}

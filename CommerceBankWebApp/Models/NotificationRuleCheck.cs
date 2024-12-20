﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Models
{
    // TODO: Add notification rules for vendor, category & location
    //used in service to check a transaction against multiple notification rules
    public class NotificationRuleCheck
    {
        public List<NotificationRule> rules;

        public NotificationRuleCheck()
        {
            rules = new List<NotificationRule>();
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
                    CommerceBankWebApp.Models.Email.SendMail(rule, transaction);
                }
            }
            return notifs;
        }

    }
    //Class to polymorph into individual rule types
    [Keyless]
    [NotMapped]
    public class NotificationRule
    {
        public AccountHolder accountHolder;
        public int id;
        public char Type;
        public string Message;
        public decimal threshold;
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
        public ThresholdRule(decimal threshold, string message, int id)
        {
            this.id = id;
            this.threshold = threshold;
            Type = 't';
            Message = message;
        }
        public override Notification ApplyRule(Transaction transaction)
        {
            Notification notif = null;
            if (transaction.Amount > this.threshold)
            {
                notif = new Notification
                {
                    Message = this.Message
                };
            }
            return notif;
        }
    }
    //Rule to send notification if transaction makes balance go negative
    public class NegativeRule : NotificationRule
    {
        public NegativeRule( string message, int id)
        {
            this.id = id;
            Type = 'n';
            Message = message;
        }
        public override Notification ApplyRule(Transaction transaction)
        {
            Notification notif = null;
            if (transaction.BankAccount.Balance - transaction.Amount < 0)
            {
                notif = new Notification
                {
                    Message = this.Message
                };
            }
            return notif;
        }
    }
    //Rule to send notification if transaction makes balance go negative
    public class DuplicateRule : NotificationRule
    {
        public DuplicateRule(string message, int id)
        {
            this.id = id;
            Type = 'd';
            Message = message;
        }
        public override Notification ApplyRule(Transaction transaction)
        {
            Notification notif = null;
            foreach (Transaction temp in transaction.BankAccount.Transactions)
            {
                if (temp.DateProcessed == transaction.DateProcessed && temp.Amount == transaction.Amount && temp.Description == transaction.Description)
                {
                    notif = new Notification
                    {
                        Message = this.Message  // + "Transaction " + temp.Id + " and Transaction " + transaction.Id
                    };
                }
            }
            return notif;
        }
    }

}

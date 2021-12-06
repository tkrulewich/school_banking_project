using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;

namespace CommerceBankWebApp.Models
{
    public class Email
    {
        public static string apiKey = "";
        public static void SendMail(NotificationRule rule, Transaction transaction)
        {
            switch (rule.Type)
            {
                case 't':
                    SendThreshHoldNotification(rule, transaction).Wait();
                    break;
                case 'n':
                    SendNegativeBalanceNotification(rule, transaction).Wait();
                    break;
                default:
                    break;
            }
        }


        // Threshold
        static async Task SendThreshHoldNotification(NotificationRule rule, Transaction transaction)

        {
            //Assign variables
            var account = transaction.BankAccount.AccountHolder;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("fcbialerts@gmail.com", "FCBI");
            var subject = "Account Notification: Large Transaction Detected";
            var to = new EmailAddress(account.EmailAddress, account.FirstName + " " + account.LastName);

            var dynamicTemplateData = new TemplateData
            {
                Subject = subject,
                Name = account.FirstName + " " + account.LastName,
                BankID = transaction.BankAccountId,
                TransactionType = transaction.TransactionTypeId == 0 ? "Withdrawal" : "Deposit",
                Amount = transaction.Amount,
                Description = transaction.Description,
                Location = transaction.Location,
                Date = transaction.DateProcessed

            };

            var msg = MailHelper.CreateSingleTemplateEmail(from, to, "d-22082f547a5047be848a2d8249f1e3bf", dynamicTemplateData);
            await client.SendEmailAsync(msg);
        }

        static async Task SendNegativeBalanceNotification(NotificationRule rule, Transaction transaction)

        {
            //Assign variables
            var account = transaction.BankAccount.AccountHolder;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("fcbialerts@gmail.com", "FCBI");
            var subject = "Account Notification: Large Transaction Detected";
            var to = new EmailAddress(account.EmailAddress, account.FirstName + " " + account.LastName);

            var dynamicTemplateData = new TemplateData
            {
                Subject = subject,
                Name = account.FirstName + " " + account.LastName,
                BankID = transaction.BankAccountId,
                TransactionType = transaction.TransactionTypeId == 0 ? "Withdrawal" : "Deposit",
                Amount = transaction.Amount,
                Description = transaction.Description,
                Location = transaction.Location,
                Date = transaction.DateProcessed,
                Balance = transaction.BankAccount.Balance

            };

            var msg = MailHelper.CreateSingleTemplateEmail(from, to, "d-5d52d5fa43ec4631a04e8a29afa95c69", dynamicTemplateData);
            await client.SendEmailAsync(msg);
        }

        private class TemplateData
        {
            [JsonProperty("Subject")]
            public string Subject { get; set; }
            [JsonProperty("Name")]
            public string Name { get; set; }
            [JsonProperty("BankID")]
            public int BankID { get; set; }
            [JsonProperty("TransactionType")]
            public string TransactionType { get; set; }
            [JsonProperty("Amount")]
            public decimal Amount { get; set; }
            [JsonProperty("Description")]
            public string Description { get; set; }
            [JsonProperty("Location")]
            public string Location { get; set; }
            [JsonProperty("Date")]
            public DateTime Date { get; set; }
            [JsonProperty("Balance")]
            public decimal Balance { get; set; }
        }
    }
}
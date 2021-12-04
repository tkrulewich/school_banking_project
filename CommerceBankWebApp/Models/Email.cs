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
        // threshold email format = "d-784bc49ed30d4e5c93f31cb649d2c1e3";
        // recover email format   = "d-bb60050f69154a419ef14528a96be6d4";
        public void SendMail(Transaction transaction, string content)
        {
            Execute(transaction, content).Wait();
        }
        // Threshold
        static async Task Execute(Transaction transaction, string content)
        {
            // Assign variables
            var account = transaction.BankAccount.AccountHolder;
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("BussyFCBI@gmail.com", "FCBI");
            var subject = content;
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

            // Send mail
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, "d-784bc49ed30d4e5c93f31cb649d2c1e3", dynamicTemplateData);
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
        }
    }
}
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
        public void SendMail(Transaction transaction, string content)
        {
            Execute(transaction, content).Wait();
        }
        // Threshold
        static async Task Execute(Transaction transaction, string content)
        {
            // Assign variables
            var account = transaction.BankAccount.AccountHolder;
            var apiKey = "";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("tkrulewich@gmail.com", "FCBI");
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
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, "", dynamicTemplateData);
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
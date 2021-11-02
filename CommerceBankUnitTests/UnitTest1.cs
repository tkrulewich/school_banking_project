using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommerceBankWebApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace CommerceBankUnitTests
{
    [TestClass]
    public class UnitTest1
    {

        string accountNumber = "78907623";

        [TestMethod]
        public void GetAllBankAccountsFromUserTest()
        {
            using (var db = new CommerceBankWebApp.Data.ApplicationDbContext(new DbContextOptionsBuilder<CommerceBankWebApp.Data.ApplicationDbContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-CommerceBankWebApp-43D51693-8F5B-4F63-855F-FE733AE31BA6;Trusted_Connection=True;MultipleActiveResultSets=true").Options))
            {
                Assert.AreNotEqual(db.GetAllBankAccountsFromUser(accountNumber).Result.Count, 0);
            }
        }
        [TestMethod]
        public void RegisterTest()
        {
            using (var db = new CommerceBankWebApp.Data.ApplicationDbContext(new DbContextOptionsBuilder<CommerceBankWebApp.Data.ApplicationDbContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-CommerceBankWebApp-43D51693-8F5B-4F63-855F-FE733AE31BA6;Trusted_Connection=True;MultipleActiveResultSets=true").Options))
            {
                var user = new IdentityUser
                {
                    UserName = "Alm@jklsdja.com",
                    Email = "Alm@jklsdja.com"

                };
                var accountHolder = new CommerceBankWebApp.Models.AccountHolder()
                {
                    FirstName = "Bob",
                    LastName = "Daly",
                    DateOfBirth = DateTime.Today,
                    EmailAddress = "Alm@jklsdja.com",
                    PhoneNumber = "9090009999",
                    DateBecameCustomer = DateTime.Today,
                    BankAccounts = new List<CommerceBankWebApp.Models.BankAccount>(),
                };

                CommerceBankWebApp::CreateAsync();
                db.Users.Create(user, "ghjsaldgdjsa8");
                db.AddNewBankAccount(accountHolder, user, accountNumber, CommerceBankWebApp.Models.BankAccountType.Checking.Id, 8034982);
                List<CommerceBankWebApp.Models.BankAccount> bankAccounts = db.GetAllBankAccountsFromUser(accountHolder.WebAppUserId).Result;
                Assert.AreNotEqual(bankAccounts.Count, 0);
            }
        }
    }
}

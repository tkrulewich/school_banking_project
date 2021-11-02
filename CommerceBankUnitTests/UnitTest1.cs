using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommerceBankWebApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommerceBankUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private ApplicationDbContext _context;
        private DbContextOptionsBuilder<ApplicationDbContext> builder;

        string accountNumber;
        void initializeTest()
        {
            builder = new DbContextOptionsBuilder<ApplicationDbContext>()
  .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-CommerceBankWebApp-43D51693-8F5B-4F63-855F-FE733AE31BA6;Trusted_Connection=True;MultipleActiveResultSets=true");

            _context = new ApplicationDbContext(builder.Options);
            accountNumber = "78907623";
        }

        [TestMethod]
        public void GetAllBankAccountsFromUserTest()
        {
            initializeTest();
            List<BankAccount> acccounts = _context.GetAllBankAccounts().Result;
            Assert.IsNotNull(acccounts);
            Assert.AreNotEqual(acccounts.Count, 0);
            
        }
        [TestMethod]
        public void RegisterTest()
        {
            initializeTest();
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
                WebAppUserId = "yusogfyagdsuilgif",
                BankAccounts = new List<CommerceBankWebApp.Models.BankAccount>(),
            };

            _context.AddNewBankAccount(accountHolder, user, accountNumber, CommerceBankWebApp.Models.BankAccountType.Checking.Id, 1);
            var bankAccounts = _context.BankAccounts.Where(u => u.AccountNumber == accountNumber).ToList();
            Assert.AreNotEqual(bankAccounts.Count, 0);
        }
    }
}

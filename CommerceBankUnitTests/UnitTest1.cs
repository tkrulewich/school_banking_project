using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.Data.Sqlite;

namespace CommerceBankUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private ApplicationDbContext _context;
        private DbContextOptionsBuilder<ApplicationDbContext> builder;
        private SqliteConnection _connection { get; set; }

        string accountNumber;

        public UnitTest1()
        {
            InitDbContext();

        }

        public void Dispose()
        {
            _connection.Close();
        }

        void InitDbContext()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection);

            _context = new ApplicationDbContext(builder.Options);

            _context.Database.EnsureCreated();


            accountNumber = "78907623";
        }

        [TestMethod]
        public void GetAllBankAccountsFromUserTest()
        {
            List<BankAccount> acccounts = _context.GetAllBankAccounts();
            Assert.IsNotNull(acccounts);
            Assert.AreNotEqual(acccounts.Count, 0);
        }
        [TestMethod]
        public void RegisterNewAccountHolderTest()
        {
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

            _context.RegisterNewAccountHolder(accountHolder);

            var accountHolderFromQuery = _context.AccountHolders.Where(ac => ac.FirstName == "Bob").FirstOrDefault();

            Assert.IsNotNull(accountHolderFromQuery);
        }
    }
}

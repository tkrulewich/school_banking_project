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

        private SeedDataModel SeedData { get; set; }
        // use this class to seed fake data in to the db for unit tests
        private class SeedDataModel
        {
            public List<IdentityUser> Users { get; set; }
            public List<AccountHolder> AccountHolders { get; set; }
            public List<BankAccount> BankAccounts { get; set; }
            public List<BankAccountType> BankAccountTypes { get; set; }
            public List<Transaction> Transactions { get; set; }
            public List<TransactionType> TransactionTypes { get; set; }

            public SeedDataModel()
            {
                BankAccountTypes = new List<BankAccountType>()
                {
                    BankAccountType.Checking,
                    BankAccountType.Savings,
                };

                TransactionTypes = new List<TransactionType>()
                {
                    TransactionType.Deposit,
                    TransactionType.Withdrawal
                };

                Users = new List<IdentityUser>()
                {
                    new IdentityUser()
                    {
                        UserName = "bob@gmail.com"
                    }
                };

                AccountHolders = new List<AccountHolder>()
                {
                    new AccountHolder()
                    {
                        FirstName = "Joe",
                        LastName = "Mamma",
                        EmailAddress = "bob@gmail.com",
                        DateOfBirth = DateTime.Parse("08/03/1994"),
                        DateBecameCustomer = DateTime.Parse("10/13/2019"),
                        PhoneNumber = "8162034578",

                        BankAccounts = new List<BankAccount>()
                        {
                            new BankAccount() {
                                AccountNumber = "1234567",
                                BankAccountType = BankAccountType.Checking,
                                Balance = 0.0,
                                DateAccountOpened = DateTime.Parse("10/13/2019"),
                            }
                        }
                    }
                };

                BankAccounts = new List<BankAccount>()
                {

                };

                Transactions = new List<Transaction>()
                {

                };
            }
        }

        void InitDbContext()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection);

            _context = new ApplicationDbContext(builder.Options);

            _context.Database.EnsureCreated();
        }

        void SeedDb()
        {
            SeedData = new SeedDataModel();

            _context.TransactionTypes.AddRange(SeedData.TransactionTypes);
            _context.SaveChanges();
            Assert.AreEqual(_context.TransactionTypes.Count(), SeedData.TransactionTypes.Count);

            _context.BankAccountTypes.AddRange(SeedData.BankAccountTypes);
            _context.SaveChanges();
            Assert.AreEqual(_context.BankAccountTypes.Count(), SeedData.BankAccountTypes.Count);

            _context.Users.AddRange(SeedData.Users);
            _context.SaveChanges();
            Assert.AreEqual(_context.Users.Count(), SeedData.Users.Count);

            _context.AccountHolders.AddRange(SeedData.AccountHolders);
            _context.SaveChanges();
            Assert.AreEqual(_context.AccountHolders.Count(), SeedData.AccountHolders.Count);


            // there may already be accounts in the context that were added when the account holders were added
            var existingBankAccounts = _context.BankAccounts.ToList();
            // add the seed data and save
            _context.BankAccounts.AddRange(SeedData.BankAccounts);
            _context.SaveChanges();
            // assert that the correct number of bank accounts now exist in the db
            Assert.AreEqual(_context.BankAccounts.Count(), SeedData.BankAccounts.Count + existingBankAccounts.Count);
            // update the list of bank accounts in seed data to include all in db in case we need them later
            SeedData.BankAccounts = _context.BankAccounts.ToList();

            // there may already be transactions in the context that were added when the account holders or bank accounts were added
            // works the same as above, but for transactions
            var existingTransactions = _context.Transactions.ToList();
            _context.Transactions.AddRange(SeedData.Transactions);
            _context.SaveChanges();
            Assert.AreEqual(_context.Transactions.Count(), SeedData.Transactions.Count + existingTransactions.Count);
            SeedData.Transactions = _context.Transactions.ToList();
        }

        [TestMethod]
        public void SeedDatabaseTest()
        {
            InitDbContext();
            SeedDb();
            Dispose();
        }


        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _connection.Close();
        }

        [TestMethod]
        public void GetAllBankAccountsFromUserTest()
        {
            InitDbContext();
            SeedDb();

            List<BankAccount> acccounts = _context.GetAllBankAccounts();
            Assert.IsNotNull(acccounts);
            Assert.AreNotEqual(acccounts.Count(), 0);

            Dispose();
        }

        [TestMethod]
        public void RegisterNewAccountHolderTest()
        {
            InitDbContext();
            SeedDb();

            var accountHolder = new AccountHolder()
            {
                FirstName = "Bob",
                LastName = "Daly",
                DateOfBirth = DateTime.Today,
                EmailAddress = "Alm@jklsdja.com",
                PhoneNumber = "9090009999",
                DateBecameCustomer = DateTime.Today,
                WebAppUserId = SeedData.Users.First().Id,
                BankAccounts = new List<BankAccount>(),
            };

            _context.RegisterNewAccountHolder(accountHolder);

            var accountHolderFromQuery = _context.AccountHolders.Where(ac => ac.FirstName == "Bob").FirstOrDefault();

            Assert.IsNotNull(accountHolderFromQuery);

            Dispose();
        }
    }
}

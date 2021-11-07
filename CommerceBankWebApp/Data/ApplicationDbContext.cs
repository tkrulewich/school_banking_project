using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommerceBankWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AccountHolder> AccountHolders { get; set; }

        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<BankAccountType> BankAccountTypes { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // this code makes AccountNumbers unique. We can not have accounts with the same account number
            builder.Entity<BankAccount>()
                .HasIndex(b => b.AccountNumber)
                .IsUnique();
        }

        public List<BankAccount> GetAllBankAccounts()
        {
            return BankAccounts
                .Include(ac => ac.BankAccountType)
                .ToList();
        }

        public List<BankAccount> GetAllBankAccountsWithTransactions()
        {
            return BankAccounts
                .Include(ac => ac.BankAccountType)
                .Include(ac => ac.Transactions)
                .ThenInclude(t => t.TransactionType)
                .ToList();
        }

        public List<BankAccount> GetAllBankAccountsFromUser(string userId)
        {
            var accounts = new List<BankAccount>();
            try
            {
                var accountHolder = AccountHolders.Where(ach => ach.WebAppUserId == userId).FirstOrDefault();

                accounts = BankAccounts.Where(b => b.AccountHolderId == accountHolder.Id)
                        .Include(ac => ac.BankAccountType).ToList();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error finding account");
            }
            return accounts;
        }

        public List<BankAccount> GetAllBankAccountsFromUserWithTransactions(string userId)
        {
            var accountHolder = AccountHolders.Where(ach => ach.WebAppUserId == userId).FirstOrDefault();
            var accounts = BankAccounts.Where(b => b.AccountHolderId == accountHolder.Id)
                    .Include(ac => ac.BankAccountType)
                    .Include(ac => ac.Transactions)
                    .ThenInclude(t => t.TransactionType).ToList();
            return accounts;
        }

        public BankAccount GetBankAccountByAccountNumber(string accountNumber)
        {
            return BankAccounts.Where(ac => ac.AccountNumber == accountNumber)
                .Include(ac => ac.BankAccountType)
                .SingleOrDefault();
        }

        public BankAccount GetBankAccountByAccountNumberWithTransactions(string accountNumber)
        {
            return BankAccounts.Where(ac => ac.AccountNumber == accountNumber)
                .Include(ac => ac.BankAccountType)
                .Include(ac => ac.Transactions)
                .ThenInclude(t => t.TransactionType)
                .SingleOrDefault();
        }
        public void RegisterNewAccountHolder(AccountHolder accountHolder, IdentityUser user, string accountNumber, int BankAccountTypeID, int accountHolderId)
        {
            try
            {
                var bankAccount = new Models.BankAccount
                {
                    AccountNumber = accountNumber,
                    BankAccountTypeId = BankAccountTypeID,
                    AccountHolderId = accountHolderId,
                    DateAccountOpened = DateTime.Today
                };
                BankAccounts.Attach(bankAccount);

                accountHolder.WebAppUserId = user.Id;

                accountHolder.BankAccounts.Add(bankAccount);
                AccountHolders.Attach(accountHolder);

                SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Account Already Existed");
            }
        }
    }
}

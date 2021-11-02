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

        public async Task<List<BankAccount>> GetAllBankAccounts()
        {
            return await BankAccounts
                .Include(ac => ac.BankAccountType)
                .ToListAsync();
        }

        public async Task<List<BankAccount>> GetAllBankAccountsWithTransactions()
        {
            return await BankAccounts
                .Include(ac => ac.BankAccountType)
                .Include(ac => ac.Transactions)
                .ThenInclude(t => t.TransactionType)
                .ToListAsync();
        }

        public async Task<List<BankAccount>> GetAllBankAccountsFromUser(string userId)
        {
            var accounts = new List<BankAccount>();
            try
            {
                var accountHolder = await AccountHolders.Where(ach => ach.WebAppUserId == userId).FirstOrDefaultAsync();
                accounts = await BankAccounts.Where(b => b.AccountHolderId == accountHolder.Id)
                        .Include(ac => ac.BankAccountType).ToListAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error finding account");
            }
            return accounts;
        }

        public async Task<List<BankAccount>> GetAllBankAccountsFromUserWithTransactions(string userId)
        {
            var accountHolder = await AccountHolders.Where(ach => ach.WebAppUserId == userId).FirstOrDefaultAsync();
            var accounts = await BankAccounts.Where(b => b.AccountHolderId == accountHolder.Id)
                    .Include(ac => ac.BankAccountType)
                    .Include(ac => ac.Transactions)
                    .ThenInclude(t => t.TransactionType).ToListAsync();
            return accounts;
        }

        public async Task<BankAccount> GetBankAccountByAccountNumber(string accountNumber)
        {
            return await BankAccounts.Where(ac => ac.AccountNumber == accountNumber)
                .Include(ac => ac.BankAccountType)
                .SingleOrDefaultAsync();
        }

        public async Task<BankAccount> GetBankAccountByAccountNumberWithTransactions(string accountNumber)
        {
            return await BankAccounts.Where(ac => ac.AccountNumber == accountNumber)
                .Include(ac => ac.BankAccountType)
                .Include(ac => ac.Transactions)
                .ThenInclude(t => t.TransactionType)
                .SingleOrDefaultAsync();
        }
        public void RegisterNewAccountHolder(AccountHolder accountHolder, IdentityUser user, string accountNumber, int BankAccountTypeID, int accountHolderId)
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

            SaveChangesAsync().Wait();
        }
    }
}

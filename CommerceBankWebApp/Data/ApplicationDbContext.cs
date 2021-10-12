using CommerceBankWebApp.Areas.Identity.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace CommerceBankWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<CommerceBankWebAppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

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

    }
}

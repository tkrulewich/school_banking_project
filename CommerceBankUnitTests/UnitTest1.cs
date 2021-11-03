using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommerceBankWebApp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;

namespace CommerceBankUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private readonly ApplicationDbContext _context;

        string accountNumber = "78907623";

        [TestMethod]
        public async void GetAllBankAccountsFromUserTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=aspnet-CommerceBankWebApp-43D51693-8F5B-4F63-855F-FE733AE31BA6;Trusted_Connection=True;MultipleActiveResultSets=true");

            ApplicationDbContext _context  = new ApplicationDbContext(builder.Options);
            List<BankAccount> acccounts = await _context.GetAllBankAccounts();
            Assert.Equals(acccounts.Count(), 0);
            
        }
    }
}

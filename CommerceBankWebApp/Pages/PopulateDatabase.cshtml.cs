using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommerceBankWebApp.Data;
using CommerceBankWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SpreadsheetLight;

namespace CommerceBankWebApp.Pages
{
    [Authorize(Roles = "admin")]
    public class PopulateDatabaseModel : PageModel
    {
        public List<BankAccount> BankAccounts { get; set; }

        private readonly ILogger<PopulateDatabaseModel> _logger;
        private readonly ApplicationDbContext _context;

        // The constructor enables logging and access to the database
        public PopulateDatabaseModel(ILogger<PopulateDatabaseModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // returns a list of all transactions in the excel file
        public async Task ReadExcelData(System.IO.Stream file) {
            List<Transaction> transactionList = new List<Transaction>();
            SLDocument document = new SLDocument(file);

            var workSheets = document.GetSheetNames();


            foreach (var sheet in workSheets)
            {
                document.SelectWorksheet(sheet);

                SLWorksheetStatistics stats = document.GetWorksheetStatistics();

                for (short i = 2; i <= stats.EndRowIndex; i++)
                {
                    string accountType = "";
                    long? accountNumber = null;
                    DateTime? processingDate = null;
                    double? balance = null;
                    bool? isCredit = null;
                    double? amount = null;
                    string description = "No desciption";

                    for (short j = 1; j <= 8; j++)
                    {
                        if (!document.HasCellValue(i, j)) continue;

                        switch (document.GetCellValueAsString(1, j))
                        {
                            case "Account Type":
                                accountType = document.GetCellValueAsString(i, j);
                                break;
                            case "Acct #":
                                accountNumber = document.GetCellValueAsInt64(i, j);
                                break;
                            case "Processing Date":
                                processingDate = document.GetCellValueAsDateTime(i, j);
                                break;
                            case "Balance":
                                balance = document.GetCellValueAsDouble(i, j);
                                break;
                            case "CR (Deposit) or DR (Withdrawal":
                            case "CR (Deposit) or DR (Withdrawal)":
                                string cellText = document.GetCellValueAsString(i, j);

                                if (cellText == "CR") isCredit = true;
                                else isCredit = false;
                                break;
                            case "Amount":
                                amount = document.GetCellValueAsDouble(i, j);
                                break;
                            case "Description 1":
                                description = document.GetCellValueAsString(i, j);
                                break;
                        }
                    }


                    if (accountNumber.HasValue && processingDate.HasValue && isCredit.HasValue && amount.HasValue)
                    {
                        var bankAccountQuery = await _context.BankAccounts.Where(ac => ac.AccountNumber == accountNumber.Value).Include(ac => ac.Transactions).ToListAsync();

                        BankAccount bankAccount;

                        if (!bankAccountQuery.Any())
                        {
                            if (String.IsNullOrEmpty(accountType)) accountType = "Checking";

                            bankAccount = new BankAccount()
                            {
                                AccountNumber = accountNumber.Value,
                                AccountType = accountType,
                                Transactions = new List<Transaction>()
                            };

                            await _context.BankAccounts.AddAsync(bankAccount);

                            await _context.SaveChangesAsync();

                            bankAccountQuery = await _context.BankAccounts.Where(ac => ac.AccountNumber == accountNumber.Value).Include(ac => ac.Transactions).ToListAsync();

                            bankAccount = bankAccountQuery.First();
                        }
                        else
                        {
                            bankAccount = bankAccountQuery.First();
                            if (accountType != bankAccount.AccountType)
                            {
                                _logger.LogWarning($"Account type wrong. Account exists already with type: {accountType} but excel data claims type {bankAccountQuery.First().AccountType}");
                                accountType = bankAccountQuery.First().AccountType;
                            }
                        }

                        _logger.LogInformation(description);
                        Transaction transaction = new Transaction()
                        {
                            ProcessingDate = processingDate.Value,
                            Balance = balance,
                            IsCredit = isCredit.Value,
                            Amount = amount.Value,
                            Description = description
                        };

                        bankAccount.Transactions.Add(transaction);

                        _context.BankAccounts.Attach(bankAccount);

                        await _context.SaveChangesAsync();
                    }
                }
            }

        }



        public async Task<IActionResult> OnGetAsync()
        {
            // add all transactions in the excel file to the database
            //await ReadExcelData("transaction_data.xlsx", "Cust A");
            //await ReadExcelData("transaction_data.xlsx", "Cust B");

            // save the changes
            //await _context.SaveChangesAsync();

            BankAccounts = await _context.BankAccounts.Include(b => b.Transactions).ToListAsync();

            return Page();
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation(Upload.FileName);


            await ReadExcelData(Upload.OpenReadStream());

            BankAccounts = await _context.BankAccounts.Include(b => b.Transactions).ToListAsync();

            return Page();
        }
    }
}

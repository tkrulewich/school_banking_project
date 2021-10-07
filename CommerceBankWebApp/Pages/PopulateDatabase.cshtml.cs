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
    // only administrator users can view this page
    [Authorize(Roles = "admin")]
    public class PopulateDatabaseModel : PageModel
    {
        // list of all bank accounts
        public List<BankAccount> BankAccounts { get; set; }

        private readonly ILogger<PopulateDatabaseModel> _logger;
        private readonly ApplicationDbContext _context;

        // The constructor enables logging and access to the database
        public PopulateDatabaseModel(ILogger<PopulateDatabaseModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Reads an excel file of user transactions and adds that data to the database
        public async Task ReadExcelData(System.IO.Stream file) {

            // create a spread sheet light document using the file provided
            SLDocument document = new SLDocument(file);

            // get a list of all work sheets in the excel file
            var workSheets = document.GetSheetNames();

            // for each sheet in the document i.e. "Cust A", "Cust B"
            // TODO: Crashes on invalid files
            foreach (var sheet in workSheets)
            {
                // select the sheet
                document.SelectWorksheet(sheet);

                // get statistics about the document i.e. number of rows, cols, etc.
                SLWorksheetStatistics stats = document.GetWorksheetStatistics();

                // for each row (1 indexed. We start with row 2 because the first row is the headings)
                for (short row = 2; row <= stats.EndRowIndex; row++)
                {
                    // data we intend to read from the curent row

                    string accountType = "Checking"; // default to Checking, but if data is provided in the excel sheet, can be changed.
                    long? accountNumber = null;
                    DateTime? processingDate = null;
                    double? balance = null;
                    bool? isCredit = null;
                    double? amount = null;
                    string description = "No desciption";

                    // for every column in the current row
                    for (short col = 1; col <= 8; col++)
                    {
                        // if there is no value, skip it
                        if (!document.HasCellValue(row, col)) continue;

                        // Find out what the column header is (first row column j)
                        // and use that column header to figure out what to do with data in the current cell
                        // TODO: HANDLE BAD DATA
                        switch (document.GetCellValueAsString(1, col))
                        {
                            case "Account Type":
                                accountType = document.GetCellValueAsString(row, col);
                                break;
                            case "Acct #":
                                accountNumber = document.GetCellValueAsInt64(row, col);
                                break;
                            case "Processing Date":
                                processingDate = document.GetCellValueAsDateTime(row, col);
                                break;
                            case "Balance":
                                balance = document.GetCellValueAsDouble(row, col);
                                break;
                            case "CR (Deposit) or DR (Withdrawal":
                            case "CR (Deposit) or DR (Withdrawal)":
                                string cellText = document.GetCellValueAsString(row, col);

                                if (cellText == "CR") isCredit = true;
                                else isCredit = false;
                                break;
                            case "Amount":
                                amount = document.GetCellValueAsDouble(row, col);
                                break;
                            case "Description 1":
                                description = document.GetCellValueAsString(row, col);
                                break;
                        }
                    }

                    // if we got values for the account #, date, Withdrawal/Credit, and the value of the transaction
                    if (accountNumber.HasValue && processingDate.HasValue && isCredit.HasValue && amount.HasValue)
                    {
                        // query for bank accounts matching the number
                        var query = await _context.BankAccounts.Where(ac => ac.AccountNumber == accountNumber.Value).Include(ac => ac.Transactions).ToListAsync();

                        // account will be stored here
                        BankAccount bankAccount;

                        // if we didnt find any matching account numbers in the database, make a new account
                        if (!query.Any())
                        {
                            bankAccount = new BankAccount()
                            {
                                AccountNumber = accountNumber.Value,
                                AccountType = accountType,
                                Transactions = new List<Transaction>()
                            };

                            await _context.BankAccounts.AddAsync(bankAccount);

                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // if there WAS an account associated with that account number already,
                            // store the account in bankAccount
                            bankAccount = query.First();

                            // if the account type doesnt match the account type of the existing account log a warning
                            if (accountType != bankAccount.AccountType)
                            {
                                _logger.LogWarning($"Account type wrong. Account exists already with type: {accountType} but excel data claims type {bankAccount.AccountType}");
                            }
                        }

                        // create a new transaction using data from the row
                        Transaction transaction = new Transaction()
                        {
                            ProcessingDate = processingDate.Value,
                            Balance = balance,
                            IsCredit = isCredit.Value,
                            Amount = amount.Value,
                            Description = description
                        };

                        // add the transaction to the account and save the changes to the database
                        bankAccount.Transactions.Add(transaction);
                        _context.BankAccounts.Attach(bankAccount);
                        await _context.SaveChangesAsync();
                    }
                }
            }

        }



        public async Task<IActionResult> OnGetAsync()
        {
            // When the page loads read all BankAccount's in the database in the list BankAccounts.
            // Note the Include method is necessary to load the associated list of transactions in each account
            BankAccounts = await _context.BankAccounts.Include(b => b.Transactions).ToListAsync();

            return Page();
        }

        [BindProperty]
        // the file that the user uploads
        public IFormFile Upload { get; set; }

        // The page Post's when a user submits a file (hopefully an excel file)
        public async Task<IActionResult> OnPostAsync()
        {
            // read the excel data of transactions the user uploaded, and populate the database with the data inside
            await ReadExcelData(Upload.OpenReadStream());

            // read all bank accounts in the database
            // Note the Include method is necessary to load the associated list of transactions in each account
            BankAccounts = await _context.BankAccounts.Include(b => b.Transactions).ToListAsync();

            return Page();
        }
    }
}

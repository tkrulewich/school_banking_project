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

                    BankAccountType accountType = BankAccountType.Checking; // default to Checking, but if data is provided in the excel sheet, can be changed.
                    string accountNumber = null;
                    DateTime? dateProcessed = null;
                    double? balance = null;
                    TransactionType transactionType = null;
                    double? amount = null;
                    string description = null;
                    string location = null;

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
                                string accountTypeStr = document.GetCellValueAsString(row, col);
                                if (accountTypeStr == "Checking") accountType = BankAccountType.Checking;
                                else if (accountTypeStr == "Savings") accountType = BankAccountType.Savings;
                                break;
                            case "Acct #":
                                accountNumber = document.GetCellValueAsString(row, col);
                                break;
                            case "Processing Date":
                                dateProcessed = document.GetCellValueAsDateTime(row, col);
                                break;
                            case "Balance":
                                balance = document.GetCellValueAsDouble(row, col);
                                break;
                            case "CR (Deposit) or DR (Withdrawal":
                            case "CR (Deposit) or DR (Withdrawal)":
                                string cellText = document.GetCellValueAsString(row, col);

                                if (cellText == "CR") transactionType = TransactionType.Credit;
                                else transactionType = TransactionType.Withdrawal;
                                break;
                            case "Amount":
                                amount = document.GetCellValueAsDouble(row, col);
                                break;
                            case "Description 1":
                                description = document.GetCellValueAsString(row, col);
                                break;
                            case "Location":
                                location = document.GetCellValueAsString(row, col);
                                break;
                        }
                    }

                    // if the data contains an account number and a date, we got something to work with
                    if (!String.IsNullOrEmpty(accountNumber) && dateProcessed.HasValue)
                    {
                        // try to get a matching bank account from the database
                        BankAccount bankAccount = await _context.GetBankAccountByAccountNumberWithTransactions(accountNumber);

                        // if we didnt find any matching account numbers in the database, make a new account
                        if (bankAccount == null)
                        {
                            // if no balance was specified use 0.0
                            if (!balance.HasValue) balance = 0.0;

                            // We will create a new bank account of balance 0.0, then create a deposit with the transaction amount equal to balance
                            transactionType = TransactionType.Credit; // so set transaction type to credit
                            amount = balance.Value; // and the amount to balance (our initial balance)

                            // create the new bank account
                            bankAccount = new BankAccount()
                            {
                                AccountNumber = accountNumber,
                                BankAccountTypeId = accountType.Id,
                                Balance = 0.0,
                                DateAccountOpened = dateProcessed.Value,
                                Transactions = new List<Transaction>()
                            };

                            // if no description was provided, use (initial starting balance) for the first transactiond description
                            if (String.IsNullOrEmpty(description)) description = "(initial starting balance)";

                            // add the bank account the the database
                            await _context.BankAccounts.AddAsync(bankAccount);

                            await _context.SaveChangesAsync();
                        }

                        // if the transaction has an ammount and we know whether its a credit or withdrawal (we already know its got an acct# and date)
                        if (amount.HasValue && transactionType != null)
                        {
                            // if the account type doesnt match the account type of the existing account log a warning
                            if (accountType.Id != bankAccount.BankAccountTypeId)
                            {
                                _logger.LogWarning($"Account type wrong. Account exists already with type: {accountType.Description} but excel data claims type {bankAccount.BankAccountType.Description}");
                            }

                            if (String.IsNullOrEmpty(description)) description = "No description";

                            // create a new transaction using data
                            // (some of this data may have been modified above if this was the first transaction under this acct#)
                            Transaction transaction = new Transaction()
                            {
                                DateProcessed = dateProcessed.Value,
                                TransactionTypeId = transactionType.Id,
                                Amount = amount.Value,
                                Description = description,
                                Location = location
                            };

                            // Add the transaction to the database
                            bankAccount.Transactions.Add(transaction);
                            if (transactionType == TransactionType.Credit) bankAccount.Balance += transaction.Amount;
                            else bankAccount.Balance -= transaction.Amount;

                            _context.BankAccounts.Attach(bankAccount);
                            await _context.SaveChangesAsync();
                        }
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

            // redirect to the ViewTransactions page
            return RedirectToPage("ViewTransactions");
        }
    }
}

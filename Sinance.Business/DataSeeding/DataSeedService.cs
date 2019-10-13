using Serilog;
using Sinance.Business.Handlers;
using Sinance.Business.Services.Authentication;
using Sinance.Domain.Entities;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding
{
    public class DataSeedService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger _logger;
        private readonly Random _random = new Random();
        private readonly Func<IUnitOfWork> _unitOfWork;

        public DataSeedService(
            ILogger logger,
            IAuthenticationService authenticationService,
            Func<IUnitOfWork> unitOfWork)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
        }

        public async Task SeedData()
        {
            using var unitOfWork = _unitOfWork();

            const string demoUserName = "DemoUser";
            var user = await unitOfWork.UserRepository.FindSingleTracked(x => x.Username == demoUserName);
            if (user != null)
            {
                _logger.Information("DemoUser already exists, will delete all existing data for demo user");
            }
            else
            {
                _logger.Information("Creating demo user");
                await _authenticationService.CreateUser(demoUserName, demoUserName);
                user = await unitOfWork.UserRepository.FindSingleTracked(x => x.Username == demoUserName);
            }

            await DeleteExistingBankAccounts(unitOfWork, user);

            _logger.Information("Creating demo bank accounts");
            var mainBankAccount = InsertBankAccount(unitOfWork, user, "Checking 1", BankAccountType.Checking);
            var secondaryBankAccount = InsertBankAccount(unitOfWork, user, "Checking 2", BankAccountType.Checking);
            var savingsAccount = InsertBankAccount(unitOfWork, user, "Savings", BankAccountType.Savings);
            var investmentAccount = InsertBankAccount(unitOfWork, user, "Investments", BankAccountType.Investment);

            await DeleteCategoriesAndTransactions(unitOfWork, user);
            InsertCategoriesAndTransactions(unitOfWork, user, mainBankAccount, secondaryBankAccount, savingsAccount, investmentAccount);

            await unitOfWork.SaveAsync();

            await TransactionHandler.UpdateCurrentBalance(unitOfWork, mainBankAccount.Id, user.Id);
            await TransactionHandler.UpdateCurrentBalance(unitOfWork, secondaryBankAccount.Id, user.Id);
            await TransactionHandler.UpdateCurrentBalance(unitOfWork, savingsAccount.Id, user.Id);
            await TransactionHandler.UpdateCurrentBalance(unitOfWork, investmentAccount.Id, user.Id);

            _logger.Information("Data seed completed, login with DemoUser/DemoUser");
        }

        private async Task DeleteCategoriesAndTransactions(IUnitOfWork unitOfWork, SinanceUser user)
        {
            _logger.Information("Deleting existing demo categories and transactions");
            var existingCategories = await unitOfWork.CategoryRepository.FindAll(x => x.UserId == user.Id);
            unitOfWork.CategoryRepository.DeleteRange(existingCategories);

            var existingTransactions = await unitOfWork.TransactionRepository.FindAll(x => x.UserId == user.Id);
            unitOfWork.TransactionRepository.DeleteRange(existingTransactions);
            await unitOfWork.SaveAsync();
        }

        private async Task DeleteExistingBankAccounts(IUnitOfWork unitOfWork, SinanceUser user)
        {
            _logger.Information("Deleting existing demo bank accounts");

            var existingBankAccounts = await unitOfWork.BankAccountRepository.FindAll(x => x.UserId == user.Id);
            unitOfWork.BankAccountRepository.DeleteRange(existingBankAccounts);
            await unitOfWork.SaveAsync();
        }

        private BankAccount InsertBankAccount(IUnitOfWork unitOfWork, SinanceUser user, string name, BankAccountType bankAccountType)
        {
            var bankAccount = new BankAccount
            {
                AccountType = bankAccountType,
                IncludeInProfitLossGraph = true,
                StartBalance = 3000,
                Name = name,
                User = user
            };

            unitOfWork.BankAccountRepository.Insert(bankAccount);

            return bankAccount;
        }

        private void InsertCategoriesAndTransactions(IUnitOfWork unitOfWork, SinanceUser user, BankAccount primaryChecking, BankAccount secondaryChecking, BankAccount savings, BankAccount investments)
        {
            _logger.Information("Creating demo categories and transactions");

            var essentialsCategory = InsertCategory(unitOfWork, user, "Essentials", false);

            var foodCategory = InsertCategory(unitOfWork, user, "Food", false, essentialsCategory);
            InsertWeeklyTransactionsForCategory(unitOfWork, primaryChecking, foodCategory, DayOfWeek.Saturday, "FoodMarket", "Groceries", -60, -40);

            var salaryCategory = InsertCategory(unitOfWork, user, "Salary", true, essentialsCategory);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, salaryCategory, 25, "Salary", "Company", 2000, 2000);

            var electricityAndGasCategory = InsertCategory(unitOfWork, user, "Electricity and Gas", true, essentialsCategory);
            var waterCategory = InsertCategory(unitOfWork, user, "Water", true, essentialsCategory);
            var internetCategory = InsertCategory(unitOfWork, user, "Internet", true, essentialsCategory);

            var clothesCategory = InsertCategory(unitOfWork, user, "Clothes", false);
            var electronicsCategory = InsertCategory(unitOfWork, user, "Electronics", false);

            var hobbyCategory = InsertCategory(unitOfWork, user, "Hobby", false);
            var gamesCategory = InsertCategory(unitOfWork, user, "Games", false, hobbyCategory);
            var knittingCategory = InsertCategory(unitOfWork, user, "Knitting", false, hobbyCategory);

            var subscriptionsCategory = InsertCategory(unitOfWork, user, "Subscriptions", true);
            var netflixCategory = InsertCategory(unitOfWork, user, "Netflix", true, subscriptionsCategory);

            var internalCashflowCategory = InsertCategory(unitOfWork, user, "InternalCashFlow", false);
        }

        private Category InsertCategory(IUnitOfWork unitOfWork, SinanceUser demoUser, string categoryName, bool isRegular, Category parentCategory = null)
        {
            var category = new Category
            {
                Name = categoryName,
                User = demoUser,
                ParentCategory = parentCategory,
                ColorCode = string.Format("#{0:X6}", _random.Next(0x1000000)),
                IsRegular = isRegular
            };

            unitOfWork.CategoryRepository.Insert(category);

            return category;
        }

        private List<Transaction> InsertMonthlyTransactionsForCategory(IUnitOfWork unitOfWork, BankAccount bankAccount, Category category, int dayInMonth,
                    string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
        {
            var today = DateTime.Now.Date;

            DateTime startDate = new DateTime(today.Year, today.Month, dayInMonth);
            // Make sure its a historical transaction
            if (today.Day < dayInMonth)
            {
                startDate = startDate.AddMonths(-1);
            }

            var transactions = new List<Transaction>();

            // Insert 2 years of monthly transactions
            for (var i = 0; i < 12; i++)
            {
                var amount = (decimal)_random.Next(amountMinValue * 100, amountMaxValue * 100) / 100;

                transactions.Add(new Transaction
                {
                    AccountNumber = "NL02ABNA9450889198",
                    Amount = amount,
                    AmountIsNegative = true,
                    BankAccount = bankAccount,
                    Description = transactionDescription,
                    Date = startDate.AddMonths(-i),
                    TransactionCategories = new List<TransactionCategory>
                    {
                        new TransactionCategory
                        {
                            Amount = amount,
                            Category = category
                        }
                    },
                    DestinationAccount = "NL83RABO2338418883",
                    Name = transactionName,
                    User = bankAccount.User
                });
            }
            unitOfWork.TransactionRepository.InsertRange(transactions);

            return transactions;
        }

        private List<Transaction> InsertWeeklyTransactionsForCategory(IUnitOfWork unitOfWork, BankAccount bankAccount, Category category, DayOfWeek transactionDayOfWeek,
            string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
        {
            var today = DateTime.Now.Date;

            // Make sure its a historical transaction
            var todayDayOfWeek = today.DayOfWeek;
            var dayDifference = todayDayOfWeek > transactionDayOfWeek ? transactionDayOfWeek - todayDayOfWeek : transactionDayOfWeek - todayDayOfWeek - 7;
            var startDate = today.AddDays(dayDifference);

            var transactions = new List<Transaction>();

            // Insert 2 years of weekly transactions
            for (var i = 0; i < 104; i++)
            {
                var amount = (decimal)_random.Next(amountMinValue * 100, amountMaxValue * 100) / 100;

                transactions.Add(new Transaction
                {
                    AccountNumber = "NL02ABNA9450889198",
                    Amount = amount,
                    AmountIsNegative = true,
                    BankAccount = bankAccount,
                    Description = transactionDescription,
                    Date = startDate.AddDays(-7 * i),
                    TransactionCategories = new List<TransactionCategory>
                    {
                        new TransactionCategory
                        {
                            Amount = amount,
                            Category = category
                        }
                    },
                    DestinationAccount = "NL83RABO2338418883",
                    Name = transactionName,
                    User = bankAccount.User
                });
            }
            unitOfWork.TransactionRepository.InsertRange(transactions);

            return transactions;
        }
    }
}
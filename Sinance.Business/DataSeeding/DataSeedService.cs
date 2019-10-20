using Serilog;
using Sinance.Business.Handlers;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
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

        public async Task SeedData(bool overwrite)
        {
            using var unitOfWork = _unitOfWork();

            const string demoUserName = "DemoUser";
            var user = await unitOfWork.UserRepository.FindSingleTracked(x => x.Username == demoUserName);
            if (user != null)
            {
                if (overwrite)
                {
                    _logger.Information("DemoUser already exists, will delete all existing data for demo user");
                }
                else
                {
                    _logger.Information("DemoUser already exists, overwrite is disabled. Not seeding new data");
                    return;
                }
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
            InsertCategoriesAndTransactions(unitOfWork, user, mainBankAccount, savingsAccount);

            await DeleteINGImportBank(unitOfWork);
            InsertINGImportBank(unitOfWork);

            await unitOfWork.SaveAsync();

            await TransactionHandler.UpdateCurrentBalance(unitOfWork, mainBankAccount.Id, user.Id);
            await TransactionHandler.UpdateCurrentBalance(unitOfWork, secondaryBankAccount.Id, user.Id);
            await TransactionHandler.UpdateCurrentBalance(unitOfWork, savingsAccount.Id, user.Id);
            await TransactionHandler.UpdateCurrentBalance(unitOfWork, investmentAccount.Id, user.Id);

            _logger.Information("Data seed completed, login with DemoUser/DemoUser");
        }

        private async Task DeleteCategoriesAndTransactions(IUnitOfWork unitOfWork, SinanceUserEntity user)
        {
            _logger.Information("Deleting existing demo categories and transactions");
            var existingCategories = await unitOfWork.CategoryRepository.FindAll(x => x.UserId == user.Id);
            unitOfWork.CategoryRepository.DeleteRange(existingCategories);

            var existingTransactions = await unitOfWork.TransactionRepository.FindAll(x => x.UserId == user.Id);
            unitOfWork.TransactionRepository.DeleteRange(existingTransactions);
            await unitOfWork.SaveAsync();
        }

        private async Task DeleteExistingBankAccounts(IUnitOfWork unitOfWork, SinanceUserEntity user)
        {
            _logger.Information("Deleting existing demo bank accounts");

            var existingBankAccounts = await unitOfWork.BankAccountRepository.FindAll(x => x.UserId == user.Id);
            unitOfWork.BankAccountRepository.DeleteRange(existingBankAccounts);
            await unitOfWork.SaveAsync();
        }

        private async Task DeleteINGImportBank(IUnitOfWork unitOfWork)
        {
            var existingImportBanks = await unitOfWork.ImportBankRepository.FindAll(x => x.Name == "ING");
            unitOfWork.ImportBankRepository.DeleteRange(existingImportBanks);
            await unitOfWork.SaveAsync();
        }

        private BankAccountEntity InsertBankAccount(IUnitOfWork unitOfWork, SinanceUserEntity user, string name, BankAccountType bankAccountType)
        {
            var bankAccount = new BankAccountEntity
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

        private void InsertCategoriesAndTransactions(IUnitOfWork unitOfWork, SinanceUserEntity user, BankAccountEntity primaryChecking, BankAccountEntity savingsAccount)
        {
            _logger.Information("Creating demo categories and transactions");

            // TODO: This should be a standard category, already created issue
            //var essentialsCategory = InsertCategory(unitOfWork, user, "Essentials", false);

            var mortgageCategory = InsertCategory(unitOfWork, user, "Mortgage", true);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, mortgageCategory, 3, "Mortgage payment", "Bank", -1000, -1000);

            var foodCategory = InsertCategory(unitOfWork, user, "Food", false);
            InsertWeeklyTransactionsForCategory(unitOfWork, primaryChecking, foodCategory, DayOfWeek.Saturday, "FoodMarket", "Groceries", -60, -40);
            InsertRandomMonthlyTransactionsForCategory(unitOfWork, primaryChecking, foodCategory, "Dinner for 2", "Restaurant", -100, -75);

            var salaryCategory = InsertCategory(unitOfWork, user, "Salary", true);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, salaryCategory, 25, "Salary", "Company", 2000, 2000);

            var electricityAndGasCategory = InsertCategory(unitOfWork, user, "Electricity and Gas", true);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, electricityAndGasCategory, 4, "Electricity and Gas", "Electricity and Gas Company", -120, -120);

            var waterCategory = InsertCategory(unitOfWork, user, "Water", true);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, waterCategory, 8, "Water", "Water Company", -30, -30);

            var internetCategory = InsertCategory(unitOfWork, user, "Internet", true);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, internetCategory, 25, "Internet", "Internet Company", -60, -60);

            var clothesCategory = InsertCategory(unitOfWork, user, "Clothes", false);
            InsertRandomMonthlyTransactionsForCategory(unitOfWork, primaryChecking, clothesCategory, "Clothes", "Clothes store", -200, -50);

            var electronicsCategory = InsertCategory(unitOfWork, user, "Electronics", false);
            InsertRandomMonthlyTransactionsForCategory(unitOfWork, primaryChecking, electronicsCategory, "Electronics", "Electronics store", -100, -50);

            var hobbyCategory = InsertCategory(unitOfWork, user, "Hobby", false);
            var gamesCategory = InsertCategory(unitOfWork, user, "Games", false, hobbyCategory);
            InsertRandomMonthlyTransactionsForCategory(unitOfWork, primaryChecking, gamesCategory, "Games", "Games store", -50, -10);

            var knittingCategory = InsertCategory(unitOfWork, user, "Knitting", false, hobbyCategory);
            InsertRandomMonthlyTransactionsForCategory(unitOfWork, primaryChecking, knittingCategory, "Knitting", "Knitting store", -60, -10);

            var subscriptionsCategory = InsertCategory(unitOfWork, user, "Subscriptions", true);
            var netflixCategory = InsertCategory(unitOfWork, user, "Netflix", true, subscriptionsCategory);
            InsertMonthlyTransactionsForCategory(unitOfWork, primaryChecking, netflixCategory, 25, "Netflix", "Netflix subscription", -8, -8);

            // TODO: This should be a standard category, already created issue
            var internalCashflowCategory = InsertCategory(unitOfWork, user, "InternalCashFlow", false);
            InsertMonthlySavingTransaction(unitOfWork, primaryChecking, savingsAccount, internalCashflowCategory, 26, 100);
        }

        private CategoryEntity InsertCategory(IUnitOfWork unitOfWork, SinanceUserEntity demoUser, string categoryName, bool isRegular, CategoryEntity parentCategory = null)
        {
            var category = new CategoryEntity
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

        private void InsertINGImportBank(IUnitOfWork unitOfWork)
        {
            var importBank = new ImportBankEntity
            {
                Delimiter = ",",
                ImportContainsHeader = true,
                Name = "ING",
                ImportMappings = new List<ImportMappingEntity>
                {
                    new ImportMappingEntity
                    {
                        ColumnIndex = 0,
                        ColumnName = "Datum",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Date,
                        FormatValue = "yyyyMMdd"
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 1,
                        ColumnName = "Naam / Omschrijving",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Name,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 2,
                        ColumnName = "Rekening",
                        ColumnTypeId = Communication.Model.Import.ColumnType.BankAccountFrom,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 3,
                        ColumnName = "Tegenrekening",
                        ColumnTypeId = Communication.Model.Import.ColumnType.DestinationAccount,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 5,
                        ColumnName = "Af Bij",
                        ColumnTypeId = Communication.Model.Import.ColumnType.AddSubtract,
                        FormatValue = "Af"
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 6,
                        ColumnName = "Bedrag",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Amount,
                        FormatValue = null
                    },
                    new ImportMappingEntity
                    {
                        ColumnIndex = 8,
                        ColumnName = "Mededelingen",
                        ColumnTypeId = Communication.Model.Import.ColumnType.Description,
                        FormatValue = null
                    }
                }
            };
            unitOfWork.ImportBankRepository.Insert(importBank);
        }

        private void InsertMonthlySavingTransaction(IUnitOfWork unitOfWork, BankAccountEntity primaryChecking, BankAccountEntity savingsAccount,
            CategoryEntity internalCashflowCategory, int dayOfMonth, int amount)
        {
            var today = DateTime.Now.Date;

            DateTime startDate = new DateTime(today.Year, today.Month, dayOfMonth);
            // Make sure its a historical transaction
            if (today.Day < dayOfMonth)
            {
                startDate = startDate.AddMonths(-1);
            }

            var transactions = new List<TransactionEntity>();
            // Insert 2 years of monthly transactions
            for (var i = 0; i < 24; i++)
            {
                transactions.Add(new TransactionEntity
                {
                    AccountNumber = "NL02ABNA9450889198",
                    Amount = -amount,
                    BankAccount = primaryChecking,
                    Description = "Savings deposit",
                    Date = startDate.AddMonths(-i),
                    TransactionCategories = new List<TransactionCategoryEntity>
                    {
                        new TransactionCategoryEntity
                        {
                            Amount = amount,
                            Category = internalCashflowCategory
                        }
                    },
                    DestinationAccount = "NL83RABO2338418883",
                    Name = "Savings",
                    User = primaryChecking.User
                });

                transactions.Add(new TransactionEntity
                {
                    AccountNumber = "NL83RABO2338418883",
                    Amount = amount,
                    BankAccount = savingsAccount,
                    Description = "Savings deposit",
                    Date = startDate.AddMonths(-i),
                    TransactionCategories = new List<TransactionCategoryEntity>
                    {
                        new TransactionCategoryEntity
                        {
                            Amount = amount,
                            Category = internalCashflowCategory
                        }
                    },
                    DestinationAccount = "",
                    Name = "Savings",
                    User = savingsAccount.User
                });
            }
            unitOfWork.TransactionRepository.InsertRange(transactions);
        }

        private List<TransactionEntity> InsertMonthlyTransactionsForCategory(IUnitOfWork unitOfWork, BankAccountEntity bankAccount, CategoryEntity category, int dayInMonth,
                    string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
        {
            var today = DateTime.Now.Date;

            DateTime startDate = new DateTime(today.Year, today.Month, dayInMonth);
            // Make sure its a historical transaction
            if (today.Day < dayInMonth)
            {
                startDate = startDate.AddMonths(-1);
            }

            var transactions = new List<TransactionEntity>();

            // Insert 2 years of monthly transactions
            for (var i = 0; i < 24; i++)
            {
                var amount = (decimal)_random.Next(amountMinValue * 100, amountMaxValue * 100) / 100;

                transactions.Add(new TransactionEntity
                {
                    AccountNumber = "NL02ABNA9450889198",
                    Amount = amount,
                    BankAccount = bankAccount,
                    Description = transactionDescription,
                    Date = startDate.AddMonths(-i),
                    TransactionCategories = new List<TransactionCategoryEntity>
                    {
                        new TransactionCategoryEntity
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

        private List<TransactionEntity> InsertRandomMonthlyTransactionsForCategory(IUnitOfWork unitOfWork,
            BankAccountEntity bankAccount, CategoryEntity category, string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
        {
            var today = DateTime.Now.Date;

            var dayInMonth = _random.Next(1, 25);

            DateTime transactionDate = new DateTime(today.Year, today.Month, dayInMonth);
            // Make sure its a historical transaction
            if (today.Day < dayInMonth)
            {
                transactionDate = transactionDate.AddMonths(-1);
            }

            var transactions = new List<TransactionEntity>();

            // Insert 2 years of monthly transactions
            for (var i = 0; i < 24; i++)
            {
                var amount = (decimal)_random.Next(amountMinValue * 100, amountMaxValue * 100) / 100;

                dayInMonth = _random.Next(-7, 7);
                transactionDate = transactionDate.AddDays(dayInMonth);

                transactions.Add(new TransactionEntity
                {
                    AccountNumber = "NL02ABNA9450889198",
                    Amount = amount,
                    BankAccount = bankAccount,
                    Description = transactionDescription,
                    Date = transactionDate,
                    TransactionCategories = new List<TransactionCategoryEntity>
                    {
                        new TransactionCategoryEntity
                        {
                            Amount = amount,
                            Category = category
                        }
                    },
                    DestinationAccount = "NL83RABO2338418883",
                    Name = transactionName,
                    User = bankAccount.User
                });

                transactionDate = transactionDate.AddMonths(-1);
            }

            unitOfWork.TransactionRepository.InsertRange(transactions);

            return transactions;
        }

        private List<TransactionEntity> InsertWeeklyTransactionsForCategory(IUnitOfWork unitOfWork, BankAccountEntity bankAccount, CategoryEntity category, DayOfWeek transactionDayOfWeek,
            string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
        {
            var today = DateTime.Now.Date;

            // Make sure its a historical transaction
            var todayDayOfWeek = today.DayOfWeek;
            var dayDifference = todayDayOfWeek > transactionDayOfWeek ? transactionDayOfWeek - todayDayOfWeek : transactionDayOfWeek - todayDayOfWeek - 7;
            var startDate = today.AddDays(dayDifference);

            var transactions = new List<TransactionEntity>();

            // Insert 2 years of weekly transactions
            for (var i = 0; i < 104; i++)
            {
                var amount = (decimal)_random.Next(amountMinValue * 100, amountMaxValue * 100) / 100;

                transactions.Add(new TransactionEntity
                {
                    AccountNumber = "NL02ABNA9450889198",
                    Amount = amount,
                    BankAccount = bankAccount,
                    Description = transactionDescription,
                    Date = startDate.AddDays(-7 * i),
                    TransactionCategories = new List<TransactionCategoryEntity>
                    {
                        new TransactionCategoryEntity
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
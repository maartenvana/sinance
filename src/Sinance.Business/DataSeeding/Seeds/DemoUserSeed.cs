using Microsoft.EntityFrameworkCore;
using Serilog;
using Sinance.Business.Calculations;
using Sinance.Business.Constants;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding.Seeds;

public class DemoUserSeed
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly Random _random;

    public DemoUserSeed(
        IAuthenticationService authenticationService,
        IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _random = new Random();
        _authenticationService = authenticationService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task SeedData(bool overwrite)
    {
        using var context = _dbContextFactory.CreateDbContext();

        const string demoUserName = "DemoUser";
        var user = await context.Users.SingleOrDefaultAsync(x => x.Username == demoUserName);
        if (user != null)
        {
            if (!overwrite)
            {
                Log.Information("DemoUser already exists, overwrite is disabled. Not seeding new data");
                return;
            }

            Log.Information("DemoUser already exists, will delete all existing data for demo user");
        }
        else
        {
            Log.Information("Creating demo user");
            await _authenticationService.CreateUser(demoUserName, demoUserName);
            user = await context.Users.SingleOrDefaultAsync(x => x.Username == demoUserName);
        }

        context.OverwriteUserIdProvider(new SeedUserIdProvider(user.Id));

        await DeleteExistingBankAccounts(context);
        await CategorySeed.SeedStandardCategoriesForUser(context, user.Id);

        await context.SaveChangesAsync();

        Log.Information("Creating demo bank accounts");
        var mainBankAccount = await InsertBankAccountAsync(context, user, "Checking 1", BankAccountType.Checking);
        var secondaryBankAccount = await InsertBankAccountAsync(context, user, "Checking 2", BankAccountType.Checking);
        var savingsAccount = await InsertBankAccountAsync(context, user, "Savings", BankAccountType.Savings);
        var investmentAccount = await InsertBankAccountAsync(context, user, "Investments", BankAccountType.Investment);

        await DeleteCategoriesAndTransactions(context);
        await InsertCategoriesAndTransactions(context, user, mainBankAccount, savingsAccount);

        await BankAccountCalculations.UpdateCurrentBalanceForBankAccount(context, mainBankAccount.Id);
        await BankAccountCalculations.UpdateCurrentBalanceForBankAccount(context, secondaryBankAccount.Id);
        await BankAccountCalculations.UpdateCurrentBalanceForBankAccount(context, savingsAccount.Id);
        await BankAccountCalculations.UpdateCurrentBalanceForBankAccount(context, investmentAccount.Id);

        await context.SaveChangesAsync();

        Log.Information("Data seed completed, login with DemoUser/DemoUser");
    }

    private static async Task DeleteCategoriesAndTransactions(SinanceContext context)
    {
        Log.Information("Deleting existing demo categories and transactions");
        var existingCategories = await context.Categories.Where(x => !x.IsStandard).ToListAsync();
        context.Categories.RemoveRange(existingCategories);

        var existingTransactions = await context.Transactions.ToListAsync();
        context.Transactions.RemoveRange(existingTransactions);
        await context.SaveChangesAsync();
    }

    private static async Task DeleteExistingBankAccounts(SinanceContext context)
    {
        Log.Information("Deleting existing demo bank accounts");

        var existingBankAccounts = await context.BankAccounts.ToListAsync();
        context.BankAccounts.RemoveRange(existingBankAccounts);
        await context.SaveChangesAsync();
    }

    private static async Task<BankAccountEntity> InsertBankAccountAsync(SinanceContext context, SinanceUserEntity user, string name, BankAccountType bankAccountType)
    {
        var bankAccount = new BankAccountEntity
        {
            AccountType = bankAccountType,
            StartBalance = 3000,
            Name = name,
            User = user
        };

        await context.BankAccounts.AddAsync(bankAccount);

        return bankAccount;
    }

    private async Task InsertCategoriesAndTransactions(SinanceContext context, SinanceUserEntity user, BankAccountEntity primaryChecking, BankAccountEntity savingsAccount)
    {
        Log.Information("Creating demo categories and transactions");

        var mortgageCategory = await InsertCategoryAsync(context, user, "Mortgage", true);
        await InsertMonthlyTransactionsForCategoryAsync(context, primaryChecking, mortgageCategory, 3, "Mortgage payment", "Bank", -1000, -1000);

        var foodCategory = await InsertCategoryAsync(context, user, "Food", false);
        await InsertWeeklyTransactionsForCategoryAsync(context, primaryChecking, foodCategory, DayOfWeek.Saturday, "FoodMarket", "Groceries", -60, -40);
        await InsertRandomMonthlyTransactionsForCategoryAsync(context, primaryChecking, foodCategory, "Dinner for 2", "Restaurant", -100, -75);

        var salaryCategory = await InsertCategoryAsync(context, user, "Salary", true);
        await InsertMonthlyTransactionsForCategoryAsync(context, primaryChecking, salaryCategory, 25, "Salary", "Company", 2000, 2000);

        var electricityAndGasCategory = await InsertCategoryAsync(context, user, "Electricity and Gas", true);
        await InsertMonthlyTransactionsForCategoryAsync(context, primaryChecking, electricityAndGasCategory, 4, "Electricity and Gas", "Electricity and Gas Company", -120, -120);

        var waterCategory = await InsertCategoryAsync(context, user, "Water", true);
        await InsertMonthlyTransactionsForCategoryAsync(context, primaryChecking, waterCategory, 8, "Water", "Water Company", -30, -30);

        var internetCategory = await InsertCategoryAsync(context, user, "Internet", true);
        await InsertMonthlyTransactionsForCategoryAsync(context, primaryChecking, internetCategory, 25, "Internet", "Internet Company", -60, -60);

        var clothesCategory = await InsertCategoryAsync(context, user, "Clothes", false);
        await InsertRandomMonthlyTransactionsForCategoryAsync(context, primaryChecking, clothesCategory, "Clothes", "Clothes store", -200, -50);

        var electronicsCategory = await InsertCategoryAsync(context, user, "Electronics", false);
        await InsertRandomMonthlyTransactionsForCategoryAsync(context, primaryChecking, electronicsCategory, "Electronics", "Electronics store", -100, -50);

        var hobbyCategory = await InsertCategoryAsync(context, user, "Hobby", false);
        var gamesCategory = await InsertCategoryAsync(context, user, "Games", false, hobbyCategory);
        await InsertRandomMonthlyTransactionsForCategoryAsync(context, primaryChecking, gamesCategory, "Games", "Games store", -50, -10);

        var knittingCategory = await InsertCategoryAsync(context, user, "Knitting", false, hobbyCategory);
        await InsertRandomMonthlyTransactionsForCategoryAsync(context, primaryChecking, knittingCategory, "Knitting", "Knitting store", -60, -10);

        var subscriptionsCategory = await InsertCategoryAsync(context, user, "Subscriptions", true);
        var netflixCategory = await InsertCategoryAsync(context, user, "Netflix", true, subscriptionsCategory);
        await InsertMonthlyTransactionsForCategoryAsync(context, primaryChecking, netflixCategory, 25, "Netflix", "Netflix subscription", -8, -8);

        var internalCashFlowCategory = await context.Categories.SingleOrDefaultAsync(x => x.IsStandard && x.UserId == user.Id && x.Name == StandardCategoryNames.InternalCashFlowName);
        await InsertMonthlySavingTransactionAsync(context, primaryChecking, savingsAccount, internalCashFlowCategory, 26, 100);
    }

    private async Task<CategoryEntity> InsertCategoryAsync(SinanceContext context, SinanceUserEntity demoUser, string categoryName, bool isRegular, CategoryEntity parentCategory = null)
    {
        var category = new CategoryEntity
        {
            Name = categoryName,
            User = demoUser,
            ParentCategory = parentCategory,
            ColorCode = string.Format("#{0:X6}", _random.Next(0x1000000)),
            IsRegular = isRegular
        };

        await context.Categories.AddAsync(category);

        return category;
    }

    private static async Task InsertMonthlySavingTransactionAsync(SinanceContext context, BankAccountEntity primaryChecking, BankAccountEntity savingsAccount,
        CategoryEntity internalCashflowCategory, int dayOfMonth, int amount)
    {
        var today = DateTime.Now.Date;

        DateTime startDate = new(today.Year, today.Month, dayOfMonth);
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
        await context.Transactions.AddRangeAsync(transactions);
    }

    private async Task InsertMonthlyTransactionsForCategoryAsync(
        SinanceContext context, BankAccountEntity bankAccount, CategoryEntity category, int dayInMonth,
        string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
    {
        var today = DateTime.Now.Date;

        DateTime startDate = new(today.Year, today.Month, dayInMonth);
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
        await context.Transactions.AddRangeAsync(transactions);
    }

    private async Task InsertRandomMonthlyTransactionsForCategoryAsync(SinanceContext context,
        BankAccountEntity bankAccount, CategoryEntity category, string transactionName, string transactionDescription, int amountMinValue, int amountMaxValue)
    {
        var today = DateTime.Now.Date;

        var dayInMonth = _random.Next(1, 25);

        DateTime transactionDate = new(today.Year, today.Month, dayInMonth);
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

        await context.Transactions.AddRangeAsync(transactions);
    }

    private async Task InsertWeeklyTransactionsForCategoryAsync(
        SinanceContext context, BankAccountEntity bankAccount, CategoryEntity category, DayOfWeek transactionDayOfWeek,
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
        await context.Transactions.AddRangeAsync(transactions);
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Business.Calculations;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Business.Handlers;
using Sinance.Business.Import;
using Sinance.Business.Services;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.CategoryMappings;
using Sinance.Business.Services.Imports;
using Sinance.Business.Services.Transactions;
using Sinance.Storage.Entities;

namespace Sinance.Business;

public static class BusinessModule
{
    public static IServiceCollection AddBusinessModule(this IServiceCollection services)
    {
        // Other services
        services.AddTransient<IBankAccountService, BankAccountService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IImportService, ImportService>();
        services.AddTransient<ITransactionService, TransactionService>();

        // Extra services that might be needed.
        services.AddTransient<IPasswordHasher<SinanceUserEntity>, PasswordHasher<SinanceUserEntity>>();
        services.AddTransient<IBankFileImportHandler, BankFileImportHandler>();
        services.AddTransient<ICustomReportService, CustomReportService>();
        services.AddTransient<IYearlyOverviewCalculation, YearlyOverviewCalculation>();
        services.AddTransient<IExpensePercentageCalculation, ExpensePercentageCalculation>();
        services.AddTransient<IExpenseCalculation, ExpenseCalculation>();
        services.AddTransient<IIncomeCalculation, IncomeCalculation>();
        services.AddTransient<IProfitCalculation, ProfitLossCalculation>();
        services.AddTransient<IBalanceHistoryCalculation, BalanceHistoryCalculation>();
        services.AddTransient<CategorySeed>();
        services.AddTransient<DemoUserSeed>();
        services.AddTransient<ICategoryMappingService, CategoryMappingService>();

        services.AddTransient<IBankFileImporter, AbnAmroTxtFileImporter>();
        services.AddTransient<IBankFileImporter, IngBankCsvFileImporter>();
        services.AddTransient<IBankFileImporter, RabobankCsvFileImporter>();

        return services;
    }
}
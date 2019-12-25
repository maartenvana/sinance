using Autofac;
using Microsoft.AspNetCore.Identity;
using Sinance.Business.Calculations;
using Sinance.Business.DataSeeding;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Business.Services;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.CategoryMappings;
using Sinance.Business.Services.Imports;
using Sinance.Business.Services.Transactions;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Sinance.Business.Tests")]

namespace Sinance.Business
{
    public class BusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Services
            builder.RegisterType<BankAccountService>().As<IBankAccountService>();
            builder.RegisterType<CategoryService>().As<ICategoryService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();
            builder.RegisterType<TransactionService>().As<ITransactionService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();
            builder.RegisterType<CategoryMappingService>().As<ICategoryMappingService>();
            builder.RegisterType<ImportService>().As<IImportService>();

            // Calculations
            builder.RegisterType<BalanceHistoryCalculation>().As<IBalanceHistoryCalculation>();
            builder.RegisterType<ExpensePercentageCalculation>().As<IExpensePercentageCalculation>();
            builder.RegisterType<ProfitLossCalculation>().As<IProfitLossCalculation>();
            builder.RegisterType<ExpenseCalculation>().As<IExpenseCalculation>();
            builder.RegisterType<IncomeCalculation>().As<IIncomeCalculation>();

            // CalculationServices
            builder.RegisterType<BankAccountCalculationService>().As<IBankAccountCalculationService>();

            // Other
            builder.RegisterType<PasswordHasher<SinanceUserEntity>>().As<IPasswordHasher<SinanceUserEntity>>();

            // Storage
            builder.RegisterModule<StorageModule>();

            // Data seeding
            builder.RegisterType<DataSeedService>().As<IDataSeedService>();
            builder.RegisterType<DemoUserSeed>().AsSelf();
            builder.RegisterType<ImportBankSeed>().AsSelf();
            builder.RegisterType<CategorySeed>().AsSelf();
        }
    }
}
using Autofac;
using Microsoft.AspNetCore.Identity;
using Sinance.Business.Calculations;
using Sinance.Business.DataSeeding;
using Sinance.Business.Services;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.Transactions;
using Sinance.Storage;
using Sinance.Storage.Entities;

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

            // Calculations
            builder.RegisterType<BalanceHistoryCalculation>().As<IBalanceHistoryCalculation>();
            builder.RegisterType<ExpensePercentageCalculation>().As<IExpensePercentageCalculation>();
            builder.RegisterType<ProfitLossCalculation>().As<IProfitLossCalculation>();
            builder.RegisterType<ExpenseCalculation>().As<IExpenseCalculation>();
            builder.RegisterType<IncomeCalculation>().As<IIncomeCalculation>();

            // Other
            builder.RegisterType<PasswordHasher<SinanceUserEntity>>().As<IPasswordHasher<SinanceUserEntity>>();

            // Storage
            builder.RegisterModule<StorageModule>();
            builder.RegisterType<DataSeedService>().AsSelf();
        }
    }
}
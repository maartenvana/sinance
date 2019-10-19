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
            builder.RegisterType<PasswordHasher<SinanceUserEntity>>().As<IPasswordHasher<SinanceUserEntity>>();

            builder.RegisterType<BankAccountService>().As<IBankAccountService>();
            builder.RegisterType<CategoryService>().As<ICategoryService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();
            builder.RegisterType<TransactionService>().As<ITransactionService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();

            builder.RegisterType<BalanceHistoryCalculation>().As<IBalanceHistoryCalculation>();
            builder.RegisterType<ExpensePercentageCalculation>().As<IExpensePercentageCalculation>();
            builder.RegisterType<ProfitLossCalculation>().As<IProfitLossCalculation>();

            builder.RegisterType<DataSeedService>().AsSelf();

            builder.RegisterModule<StorageModule>();
        }
    }
}
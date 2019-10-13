using Autofac;
using Microsoft.AspNetCore.Identity;
using Sinance.Business.DataSeeding;
using Sinance.Business.Services;
using Sinance.Domain.Entities;

namespace Sinance.Business
{
    public class BusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PasswordHasher<SinanceUser>>().As<IPasswordHasher<SinanceUser>>();

            builder.RegisterType<BankAccountService>().As<IBankAccountService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();

            builder.RegisterType<DataSeedService>().AsSelf();
        }
    }
}
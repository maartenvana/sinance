using Autofac;
using Microsoft.AspNetCore.Identity;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Storage;

namespace Sinance.Business
{
    public class BusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PasswordHasher<SinanceUser>>().As<IPasswordHasher<SinanceUser>>();

            builder.RegisterType<BankAccountService>().As<IBankAccountService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();

            builder.RegisterModule<StorageModule>();
        }
    }
}
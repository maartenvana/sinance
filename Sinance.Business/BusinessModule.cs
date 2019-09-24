﻿using Autofac;
using Microsoft.AspNetCore.Identity;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Business
{
    public class BusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PasswordHasher<SinanceUser>>().As<IPasswordHasher<SinanceUser>>();

            builder.RegisterType<BankAccountService>().As<IBankAccountService>();
            builder.RegisterType<CustomReportService>().As<ICustomReportService>();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();

            builder.RegisterModule<StorageModule>();
        }
    }
}
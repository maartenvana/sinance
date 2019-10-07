﻿using Autofac;
using Sinance.Common;
using Microsoft.EntityFrameworkCore;
using Autofac.Features.OwnedInstances;
using System;
using Microsoft.Extensions.Logging;
using Sinance.Common.Configuration;

namespace Sinance.Storage
{
    public class StorageModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<Func<IUnitOfWork>>(x =>
            {
                var factory = x.Resolve<Func<Owned<IUnitOfWork>>>();

                return () =>
                {
                    var newUnitOfWork = factory();
                    return newUnitOfWork.Value;
                };
            });

            builder.RegisterGeneric(typeof(GenericRepository<>)).As(typeof(IGenericRepository<>));

            builder.Register(regContext =>
            {
                var appSettings = regContext.Resolve<AppSettings>();
                var contextOptionsBuilder = new DbContextOptionsBuilder();

                if (appSettings.Database.LoggingEnabled)
                {
                    contextOptionsBuilder.UseLoggerFactory(regContext.Resolve<ILoggerFactory>());
                }
                contextOptionsBuilder.UseMySql(appSettings.ConnectionStrings.Sql);

                return contextOptionsBuilder.Options;
            }).SingleInstance();

            builder.Register(regContext =>
            {
                var options = regContext.Resolve<DbContextOptions>();

                return new SinanceContext(options);
            }).AsSelf().InstancePerOwned<IUnitOfWork>();

            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
        }
    }
}
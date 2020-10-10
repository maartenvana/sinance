using Autofac;
using Autofac.Features.OwnedInstances;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sinance.Common.Configuration;
using System;

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

            builder.Register(context =>
            {
                var appSettings = context.Resolve<AppSettings>();
                var contextOptionsBuilder = new DbContextOptionsBuilder<SinanceContext>();

                if (appSettings.Database.LoggingEnabled)
                {
                    contextOptionsBuilder.UseLoggerFactory(context.Resolve<ILoggerFactory>());
                }
                contextOptionsBuilder.UseMySql(appSettings.ConnectionStrings.Sql);

                return contextOptionsBuilder.Options;
            }).SingleInstance();

            builder.Register(context =>
            {
                var options = context.Resolve<DbContextOptions<SinanceContext>>();
                var userIdProvider = context.Resolve<IUserIdProvider>();

                return new SinanceContext(options, userIdProvider);
            }).AsSelf().InstancePerOwned<IUnitOfWork>();

            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
        }
    }
}
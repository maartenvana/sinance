using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Common.Configuration;
using Sinance.Storage;
using System;
using System.Threading.Tasks;
using Serilog;
using Sinance.Business.DataSeeding;

namespace Sinance.Web.Extensions
{
    public static class IApplicationBuilderStorageExtensions
    {
        public static IApplicationBuilder ApplyDataSeed(this IApplicationBuilder appBuilder)
        {
            var appSettings = appBuilder.ApplicationServices.GetRequiredService<AppSettings>();
            var logger = appBuilder.ApplicationServices.GetRequiredService<ILogger>();
            if (appSettings.Database.SeedDemoData)
            {
                logger.Information("Database__SeedDemoData is enabled, starting seed of demo data");

                var dataSeeder = appBuilder.ApplicationServices.GetRequiredService<DataSeedService>();

                Task.Run(async () =>
                    {
                        await dataSeeder.SeedData(appSettings.Database.OverrideSeedDemoData);
                    }).Wait();
            }
            else
            {
                logger.Information("Database__SeedDemoData is disabled");
            }

            return appBuilder;
        }

        public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder appBuilder)
        {
            var unitOfWorkFunc = appBuilder.ApplicationServices.GetRequiredService<Func<IUnitOfWork>>();
            using var unitOfWork = unitOfWorkFunc();

            unitOfWork.Context.Migrate();

            return appBuilder;
        }
    }
}
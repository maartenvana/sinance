using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Business.DataSeeding;
using Sinance.Storage;
using System;
using System.Threading.Tasks;

namespace Sinance.Web.Extensions
{
    public static class IApplicationBuilderStorageExtensions
    {
        public static IApplicationBuilder ApplyDataSeed(this IApplicationBuilder appBuilder)
        {
            var dataSeeder = appBuilder.ApplicationServices.GetRequiredService<DataSeedService>();

            Task.Run(async () =>
            {
                await dataSeeder.SeedData();
            }).Wait();

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
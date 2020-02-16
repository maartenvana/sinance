using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Business.DataSeeding;
using System.Threading.Tasks;

namespace Sinance.Web.Extensions
{
    public static class IApplicationBuilderStorageExtensions
    {
        public static IApplicationBuilder ApplyDataSeed(this IApplicationBuilder appBuilder)
        {
            var dataSeeder = appBuilder.ApplicationServices.GetRequiredService<IDataSeedService>();

            Task.Run(async () =>
            {
                await dataSeeder.StartupSeed();
            }).Wait();

            return appBuilder;
        }

    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Common.Configuration;

namespace Sinance.Storage;

public static class StorageModule
{
    public static IServiceCollection AddStorageModule(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddDbContextFactory<SinanceContext>(opt => opt
            .UseMySql(appSettings.ConnectionStrings.Sql, new MySqlServerVersion("5.7"))
            .EnableSensitiveDataLogging());

        services.AddDbContext<SinanceContext>(opt => opt
            .UseMySql(appSettings.ConnectionStrings.Sql, new MySqlServerVersion("5.7"))
            .EnableSensitiveDataLogging());

        return services;
    }
}
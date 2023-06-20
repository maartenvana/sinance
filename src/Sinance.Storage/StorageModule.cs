using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sinance.Common.Configuration;
using System;

namespace Sinance.Storage;

public static class StorageModule
{
    public static IServiceCollection AddStorageModule(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddDbContextFactory<SinanceContext>(opt => opt
            .UseMySql(appSettings.ConnectionStrings.Sql, new MySqlServerVersion(new Version(5,7,42)))
            .EnableSensitiveDataLogging());

        services.AddDbContext<SinanceContext>(opt => opt
            .UseMySql(appSettings.ConnectionStrings.Sql, new MySqlServerVersion(new Version(5,7,42)))
            .EnableSensitiveDataLogging());

        return services;
    }
}
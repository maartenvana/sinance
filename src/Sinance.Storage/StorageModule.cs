using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Common.Configuration;
using Sinance.Storage.Entities;
using System;

namespace Sinance.Storage;

public static class StorageModule 
{
    public static IServiceCollection AddStorageModule(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddTransient<IGenericRepository<BankAccountEntity>, GenericRepository<BankAccountEntity>>();
        services.AddTransient<IGenericRepository<CategoryMappingEntity>, GenericRepository<CategoryMappingEntity>>();
        services.AddTransient<IGenericRepository<CategoryEntity> , GenericRepository<CategoryEntity>>();
        services.AddTransient<IGenericRepository<CustomReportCategoryEntity> , GenericRepository<CustomReportCategoryEntity>>();
        services.AddTransient<IGenericRepository<CustomReportEntity> , GenericRepository<CustomReportEntity>>(); 
        services.AddTransient<IGenericRepository<TransactionCategoryEntity> , GenericRepository<TransactionCategoryEntity>>();
        services.AddTransient<IGenericRepository<TransactionEntity> , GenericRepository<TransactionEntity>>();
        services.AddTransient<IGenericRepository<SinanceUserEntity>, GenericRepository<SinanceUserEntity>>();

        services.AddMySql<SinanceContext>(
            connectionString: appSettings.ConnectionStrings.Sql,
            ServerVersion.Parse("5.7"));

        return services;
    }
}
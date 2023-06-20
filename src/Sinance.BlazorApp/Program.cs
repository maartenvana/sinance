using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Sinance.Storage;
using System;
using System.Linq;

namespace Sinance.BlazorApp;

public class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder .UseStartup<Startup>();
        });

    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Building web host");
            var host = CreateHostBuilder(args).Build();

            CreateOrMigrateDatabase(host);
            SeedData(host);

            Log.Information("Starting web host");
            host.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void SeedData(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;

        try
        {
            var contextFactory = services.GetRequiredService<IDbContextFactory<SinanceContext>>();
            using var context = contextFactory.CreateDbContext();

            Log.Information("Seeding data");

            var allTransactions = context.Transactions
                .Include(x => x.TransactionCategories)
                .Where(x => x.TransactionCategories.Any()).ToList();
            foreach (var transaction in allTransactions)
            {
                transaction.CategoryId = transaction.TransactionCategories.First().CategoryId;
            }

            context.SaveChanges();

            Log.Information("Seeding data completed");
        }
        catch (Exception exc)
        {
            Log.Error(exc, "An error seeding the DB.");
        }
    }

    private static void CreateOrMigrateDatabase(IHost host)
    {
        using var scope = host.Services.CreateScope();

        var services = scope.ServiceProvider;

        try
        {
            var contextFactory = services.GetRequiredService<IDbContextFactory<SinanceContext>>();
            using var context = contextFactory.CreateDbContext();

            Log.Information("Checking if database needs to be migrated/created");
            var pendingMigrations = context.Database.GetPendingMigrations();
            foreach (var pendingMigration in pendingMigrations)
            {
                Log.Information("Need to apply migration: {pendingMigration}", pendingMigration);
            }
            context.Database.Migrate();

            Log.Information("Initializing database completed");
        }
        catch (Exception exc)
        {
            Log.Error(exc, "An error occurred migrating the DB.");
        }
    }
}
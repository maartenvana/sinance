using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Sinance.Storage;
using System;
using System.Globalization;

namespace Sinance.Web
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

        public static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("nl-NL");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("nl-NL");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Building web host");
                var host = CreateHostBuilder(args).Build();

                CreateOrMigrateDatabase(host);

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

        private static void CreateOrMigrateDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            var unitOfWorkFunc = services.GetRequiredService<Func<IUnitOfWork>>();
            using var unitOfWork = unitOfWorkFunc();

            Log.Information("Checking if database needs to be migrated/created");
            var pendingMigrations = unitOfWork.Context.Database.GetPendingMigrations();
            foreach (var pendingMigration in pendingMigrations)
            {
                Log.Information("Need to apply migration: {pendingMigration}", pendingMigration);
            }
            unitOfWork.Context.Database.Migrate();

            Log.Information("Initializing database completed");
        }
    }
}
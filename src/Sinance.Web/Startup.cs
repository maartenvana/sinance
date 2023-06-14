using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Sinance.Business;
using Sinance.Business.Services.Authentication;
using Sinance.Common.Configuration;
using Sinance.Storage;
using Sinance.Web.Initialization;
using Sinance.Web.Services;
using System;

namespace Sinance.Web;

public class Startup
{
    private readonly AppSettings _appSettings;

    private readonly IWebHostEnvironment _environment;

    public Startup(IWebHostEnvironment env)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true)
            .AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        _appSettings = configuration.Get<AppSettings>();

        _environment = env;
    }

    public void Configure(IApplicationBuilder appBuilder)
    {
        if (!_environment.IsDevelopment())
        {
            appBuilder.UseExceptionHandler("/Error");
            appBuilder.UseHsts();
        }

        appBuilder.UseSerilogRequestLogging();

        appBuilder.UseStaticFiles();

        appBuilder.UseRouting();
        appBuilder.UseCors();

        appBuilder.UseAuthentication();
        appBuilder.UseAuthorization();

        appBuilder.UseEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
            endpoints.MapControllerRoute(
                "default", "{controller=Home}/{action=Index}/{id?}");
        });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddSingleton(_appSettings);

        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<IUserIdProvider, UserIdProvider>();

        services.AddTransient<IStartupTask, DatabaseMigrationTask>();
        services.AddTransient<IStartupTask, DataSeedStartupTask>();

        services.AddBusinessModule();
        services.AddStorageModule(_appSettings);

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = new PathString("/Account/Login");
                options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
            });

        services.AddControllersWithViews();

        services.AddRazorPages();

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}
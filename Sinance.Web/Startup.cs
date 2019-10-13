using Autofac;
using Sinance.Business;
using Sinance.Common;
using Sinance.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Sinance.Web.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sinance.Web.Services;
using Sinance.Business.Services.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Sinance.Common.Configuration;
using Sinance.Web.Extensions;
using Serilog;
using AutofacSerilogIntegration;

namespace Sinance.Web
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            _environment = env;
        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            if (_environment.EnvironmentName == Environments.Development)
            {
                appBuilder.UseDeveloperExceptionPage();
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

            appBuilder
                .MigrateDatabase()
                .ApplyDataSeed();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var appSettings = _configuration.Get<AppSettings>();
            builder.RegisterInstance(appSettings);

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();

            builder.RegisterModule<BusinessModule>();
            builder.RegisterModule<StorageModule>();

            builder.RegisterType<SelectListHelper>().AsSelf();

            builder.RegisterLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/Account/Login");
                    options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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
}
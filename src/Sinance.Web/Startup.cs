using Autofac;
using AutofacSerilogIntegration;
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
using Sinance.Web.Extensions;
using Sinance.Web.Services;
using System;

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

            appBuilder.ApplyDataSeed();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var appSettings = _configuration.Get<AppSettings>();

            builder.RegisterInstance(appSettings);
            builder.RegisterInstance(_configuration);

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>();
            builder.RegisterType<UserIdProvider>().As<IUserIdProvider>();

            builder.RegisterModule<BusinessModule>();
            builder.RegisterModule<StorageModule>();

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
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                });

            var mvc = services.AddControllersWithViews();

            services.AddRazorPages();

            if (_environment.EnvironmentName == Environments.Development)
            {
                mvc.AddRazorRuntimeCompilation();
            }

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
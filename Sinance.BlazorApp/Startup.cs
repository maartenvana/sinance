using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sinance.Storage;
using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Providers;
using Sinance.BlazorApp.Extensions;
using Sinance.Common.Configuration;
using Sinance.BlazorApp.Business.Services;
using Blazored.Toast;
using BlazorStrap;
using Sinance.BlazorApp.Services;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System;

namespace Sinance.BlazorApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUserIdProvider, UserIdProvider>();

            var appSettings = Configuration.Get<AppSettings>();
            services.AddSingleton(appSettings);

            services.AddDatabase<SinanceContext>(opt => opt
                .UseMySql(appSettings.ConnectionStrings.Sql, new MySqlServerVersion("5.7"))
                .EnableSensitiveDataLogging());

            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IUserErrorService, UserErrorService>();
            services.AddTransient<IBankAccountService, BankAccountService>();
            services.AddTransient<ICategoryService, CategoryService>();

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddBlazoredToast();
            services.AddBlazorStrap();

            // Server Side Blazor doesn't register HttpClient by default
            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                // Setup HttpClient for server side in a client side compatible fashion
                services.AddScoped<HttpClient>(s =>
                {
                    // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                    var navman = s.GetRequiredService<NavigationManager>();
                    return new HttpClient
                    {
                        BaseAddress = new Uri(navman.BaseUri)
                    };
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

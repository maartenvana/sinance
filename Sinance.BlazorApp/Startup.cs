using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sinance.Storage;
using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Services;
using Sinance.BlazorApp.Extensions;
using Sinance.Common.Configuration;
using Sinance.BlazorApp.Business.Services;

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

            services.AddDbContextFactory<SinanceContext>(opt => opt
                .UseMySql(appSettings.ConnectionStrings.Sql)
                .EnableSensitiveDataLogging());

            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IBankAccountService, BankAccountService>();
            services.AddTransient<ICategoryService, CategoryService>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
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

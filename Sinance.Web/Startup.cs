using Autofac;
using Autofac.Extensions.DependencyInjection;
using Sinance.Business;
using Sinance.Common;
using Sinance.Domain.Entities;
using Sinance.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Sinance.Web.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Sinance.Web
{
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            _environment = env;
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            //app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                // Areas support
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public System.IServiceProvider ConfigureServices(IServiceCollection services)
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

            var mvcBuilder = services.AddMvcCore(options =>
            {
                //options.Filters.Add(typeof(AuthorizeAttribute));
            })
            .AddRazorPages()
            .AddRazorViewEngine()
            .AddAuthorization()
            .AddFormatterMappings()
            .AddJsonFormatters()
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var appSettings = _configuration.Get<AppSettings>();

            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.RegisterInstance(appSettings.ConnectionStrings);

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();

            builder.RegisterModule<BusinessModule>();

            builder.RegisterType<SelectListHelper>().AsSelf();

            var container = builder.Build();

            var unitOfWorkFunc = container.Resolve<Func<IUnitOfWork>>();
            using (var unitOfWork = unitOfWorkFunc())
            {
                unitOfWork.Context.Migrate();
            }

            return new AutofacServiceProvider(container);
        }
    }
}
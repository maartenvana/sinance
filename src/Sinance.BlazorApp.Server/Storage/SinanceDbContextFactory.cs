using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Sinance.BlazorApp.Storage
{
    public class SinanceDbContextFactory<TContext>
           : ISinanceDbContextFactory<TContext> where TContext : DbContext
    {
        private readonly IServiceProvider provider;

        public SinanceDbContextFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public TContext CreateDbContext()
        {
            if (provider == null)
            {
                throw new InvalidOperationException(
                    $"You must configure an instance of IServiceProvider");
            }

            return ActivatorUtilities.CreateInstance<TContext>(provider);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Sinance.Application
{
    public static class ApplicationModule
    {
       public static IServiceCollection RegisterApplicationModule(this IServiceCollection services) 
       {
            return services.AddMediatR(Assembly.GetExecutingAssembly());
       }
    }
}
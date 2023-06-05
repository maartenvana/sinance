using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Sinance.Web.Initialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sinance.Web.Extensions;

public static class IHostExtensions
{
    public static async Task RunWithStartupTasksAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();

        var tasks = scope.ServiceProvider.GetService<IEnumerable<IStartupTask>>();

        if (tasks?.Any() != true)
            return;

        foreach (var task in tasks!)
        {
            Log.Information("Starting startup task: {task}", task.GetType().Name);

            try
            {
                await task.RunAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Exception during startup task");
                throw;
            }

            Log.Information("Finished startup task: {task}", task.GetType().Name);
        }

        host.Run();
    }
}

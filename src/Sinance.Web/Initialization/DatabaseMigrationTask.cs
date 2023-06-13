using Microsoft.EntityFrameworkCore;
using Serilog;
using Sinance.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace Sinance.Web.Initialization;

public class DatabaseMigrationTask : IStartupTask
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public DatabaseMigrationTask(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

        foreach (var pendingMigration in pendingMigrations)
        {
            Log.Information("Need to apply migration: {pendingMigration}", pendingMigration);
        }

        await context.Database.MigrateAsync(cancellationToken);
    }
}

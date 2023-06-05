using Microsoft.EntityFrameworkCore;
using Serilog;
using Sinance.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sinance.Web.Initialization;

public class DatabaseMigrationTask : IStartupTask
{
    private readonly IUnitOfWork _unitOfWork;

    public DatabaseMigrationTask(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var pendingMigrations = await _unitOfWork.Context.Database.GetPendingMigrationsAsync(cancellationToken);

        foreach (var pendingMigration in pendingMigrations)
        {
            Log.Information("Need to apply migration: {pendingMigration}", pendingMigration);
        }

        await _unitOfWork.Context.Database.MigrateAsync(cancellationToken);
    }
}

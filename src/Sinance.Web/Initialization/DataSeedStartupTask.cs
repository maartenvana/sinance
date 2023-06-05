using Serilog;
using Sinance.Business.DataSeeding;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Common.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Sinance.Web.Initialization;

public class DataSeedStartupTask : IStartupTask
{
    private readonly AppSettings _appSettings;
    private readonly CategorySeed _categorySeed;
    private readonly DemoUserSeed _demoUserSeed;

    public DataSeedStartupTask(
        AppSettings appSettings,
        CategorySeed categorySeed, 
        DemoUserSeed demoUserSeed)
    {
        _appSettings = appSettings;
        _categorySeed = categorySeed;
        _demoUserSeed = demoUserSeed;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Log.Information("Starting database seeding");

        if (_appSettings.Database.SeedDemoData)
        {
            Log.Information("SeedDemoData is enabled, starting seed of demo data");

            await _demoUserSeed.SeedData(_appSettings.Database.OverrideSeedDemoData);
        }

        await _categorySeed.SeedStandardCategoriesForAllUsers();

        Log.Information("Database seed completed");
    }
}

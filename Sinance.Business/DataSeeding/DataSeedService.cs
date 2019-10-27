using Serilog;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Common.Configuration;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding
{
    public class DataSeedService
    {
        private readonly AppSettings _appSettings;
        private readonly DemoUserSeed _demoUserSeed;
        private readonly ImportBankSeed _importBankSeed;
        private readonly ILogger _logger;

        public DataSeedService(
            ILogger logger,
            AppSettings appSettings,
            DemoUserSeed demoUserSeed,
            ImportBankSeed importBankSeed)
        {
            _logger = logger;
            _appSettings = appSettings;
            _demoUserSeed = demoUserSeed;
            _importBankSeed = importBankSeed;
        }

        public async Task SeedData()
        {
            _logger.Information("Starting database seeding");

            if (_appSettings.Database.SeedDemoData)
            {
                _logger.Information("SeedDemoData is enabled, starting seed of demo data");

                await _demoUserSeed.SeedData(_appSettings.Database.OverrideSeedDemoData);
            }

            await _importBankSeed.SeedData();

            _logger.Information("Database seed completed");
        }
    }
}
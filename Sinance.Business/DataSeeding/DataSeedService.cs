using Serilog;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Common.Configuration;
using Sinance.Storage;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding
{
    public class DataSeedService : IDataSeedService
    {
        private readonly AppSettings _appSettings;
        private readonly CategorySeed _categorySeed;
        private readonly DemoUserSeed _demoUserSeed;
        private readonly ImportBankSeed _importBankSeed;
        private readonly ILogger _logger;

        public DataSeedService(
            ILogger logger,
            AppSettings appSettings,
            CategorySeed categorySeed,
            DemoUserSeed demoUserSeed,
            ImportBankSeed importBankSeed)
        {
            _logger = logger;
            _appSettings = appSettings;
            _categorySeed = categorySeed;
            _demoUserSeed = demoUserSeed;
            _importBankSeed = importBankSeed;
        }

        public async Task NewUserSeed(IUnitOfWork unitOfWork, int userId)
        {
            _logger.Information("Seeding new user");

            await _categorySeed.SeedStandardCategoriesForUser(unitOfWork, userId);
        }

        public async Task StartupSeed()
        {
            _logger.Information("Starting database seeding");

            if (_appSettings.Database.SeedDemoData)
            {
                _logger.Information("SeedDemoData is enabled, starting seed of demo data");

                await _demoUserSeed.SeedData(_appSettings.Database.OverrideSeedDemoData);
            }

            await _importBankSeed.SeedData();

            await _categorySeed.SeedStandardCategoriesForAllUsers();

            _logger.Information("Database seed completed");
        }
    }
}
using Serilog;
using Sinance.Business.DataSeeding.Seeds;
using Sinance.Common.Configuration;
using Sinance.Storage;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding
{
    public class DataSeedService : IDataSeedService
    {
        private readonly AppSettings _appSettings;
        private readonly CategorySeed _categorySeed;
        private readonly DemoUserSeed _demoUserSeed;
        private readonly ILogger _logger;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public DataSeedService(
            ILogger logger,
            Func<IUnitOfWork> unitOfWork,
            AppSettings appSettings,
            CategorySeed categorySeed,
            DemoUserSeed demoUserSeed)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _appSettings = appSettings;
            _categorySeed = categorySeed;
            _demoUserSeed = demoUserSeed;
        }

        public async Task NewUserSeed(int userId)
        {
            _logger.Information("Seeding new user");

            using var unitOfWork = _unitOfWork();
            unitOfWork.Context.OverwriteUserIdProvider(new SeedUserIdProvider(userId));

            await _categorySeed.SeedStandardCategoriesForUser(unitOfWork, userId);

            await unitOfWork.SaveAsync();
        }

        public async Task StartupSeed()
        {
            _logger.Information("Starting database seeding");

            if (_appSettings.Database.SeedDemoData)
            {
                _logger.Information("SeedDemoData is enabled, starting seed of demo data");

                await _demoUserSeed.SeedData(_appSettings.Database.OverrideSeedDemoData);
            }

            await _categorySeed.SeedStandardCategoriesForAllUsers();

            _logger.Information("Database seed completed");
        }
    }
}
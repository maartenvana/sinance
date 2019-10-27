using Serilog;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding.Seeds
{
    public class CategorySeed
    {
        private readonly ILogger _logger;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategorySeed(
            ILogger logger,
            Func<IUnitOfWork> unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task SeedStandardCategoriesForAllUsers()
        {
            _logger.Information("Seeding standard categories for all users");
            using var unitOfWork = _unitOfWork();

            var allUsers = await unitOfWork.UserRepository.ListAll();

            foreach (var user in allUsers)
            {
                await SeedCategories(user.Id, unitOfWork);
            }

            await unitOfWork.SaveAsync();

            _logger.Information("Standard categories seeding completed");
        }

        public async Task SeedStandardCategoriesForUser(int userId)
        {
            using var unitOfWork = _unitOfWork();

            await SeedCategories(userId, unitOfWork);

            await unitOfWork.SaveAsync();
        }

        private void CreateOrUpdateCashFlowCategory(IUnitOfWork unitOfWork, List<CategoryEntity> standardCategoriesForUser, int userId)
        {
            const string internalCashFlowName = "InternalCashFlow";

            if (!standardCategoriesForUser.Any(x => x.Name == internalCashFlowName))
            {
                unitOfWork.CategoryRepository.Insert(new CategoryEntity
                {
                    IsStandard = true,
                    IsRegular = false,
                    Name = internalCashFlowName,
                    ColorCode = "#00ff00",
                    UserId = userId
                });
            }
        }

        private async Task SeedCategories(int userId, IUnitOfWork unitOfWork)
        {
            var standardCategoriesForUser = await unitOfWork.CategoryRepository.FindAll(x => x.IsStandard && x.UserId == userId);

            CreateOrUpdateCashFlowCategory(unitOfWork, standardCategoriesForUser, userId);
        }
    }
}
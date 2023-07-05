using Serilog;
using Sinance.Business.Constants;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding.Seeds;

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
            unitOfWork.Context.OverwriteUserIdProvider(new SeedUserIdProvider(user.Id));
            await SeedStandardCategoriesForUser(unitOfWork, user.Id);
        }

        await unitOfWork.SaveAsync();

        _logger.Information("Standard categories seeding completed");
    }

    public async Task SeedStandardCategoriesForUser(IUnitOfWork unitOfWork, int userId)
    {
        var standardCategoriesForUser = await unitOfWork.CategoryRepository.FindAll(x => x.IsStandard);

        CreateOrUpdateCashFlowCategory(unitOfWork, standardCategoriesForUser, userId);
    }

    private void CreateOrUpdateCashFlowCategory(IUnitOfWork unitOfWork, List<CategoryEntity> standardCategoriesForUser, int userId)
    {
        if (!standardCategoriesForUser.Any(x => x.Name == StandardCategoryNames.InternalCashFlowName))
        {
            unitOfWork.CategoryRepository.Insert(new CategoryEntity
            {
                IsStandard = true,
                IsRegular = false,
                Name = StandardCategoryNames.InternalCashFlowName,
                ColorCode = "#00ff00",
                UserId = userId
            });
        }
    }
}
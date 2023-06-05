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
    private readonly IUnitOfWork _unitOfWork;

    public CategorySeed(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SeedStandardCategoriesForAllUsers()
    {
        Log.Information("Seeding standard categories for all users");
        

        var allUsers = await _unitOfWork.UserRepository.ListAll();

        foreach (var userId in allUsers.Select(x => x.Id))
        {
            _unitOfWork.Context.OverwriteUserIdProvider(new SeedUserIdProvider(userId));
            await SeedStandardCategoriesForUser(_unitOfWork, userId);
        }

        await _unitOfWork.SaveAsync();

        Log.Information("Standard categories seeding completed");
    }

    public async Task SeedStandardCategoriesForUser(IUnitOfWork unitOfWork, int userId)
    {
        var standardCategoriesForUser = await _unitOfWork.CategoryRepository.FindAll(x => x.IsStandard);

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
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sinance.Business.Constants;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.DataSeeding.Seeds;

public class CategorySeed
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public CategorySeed(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task SeedStandardCategoriesForAllUsers()
    {
        Log.Information("Seeding standard categories for all users");

        using var context = _dbContextFactory.CreateDbContext();

        var allUsers = await context.Users.ToListAsync();

        foreach (var userId in allUsers.Select(x => x.Id))
        {
            context.OverwriteUserIdProvider(new SeedUserIdProvider(userId));
            await SeedStandardCategoriesForUser(context, userId);
        }

        await context.SaveChangesAsync();

        Log.Information("Standard categories seeding completed");
    }

    public static async Task SeedStandardCategoriesForUser(SinanceContext context, int userId)
    {
        var standardCategoriesForUser = await context.Categories.Where(x => x.IsStandard).ToListAsync();

        await CreateOrUpdateCashFlowCategory(context, standardCategoriesForUser, userId);
    }

    private static async Task CreateOrUpdateCashFlowCategory(SinanceContext context, List<CategoryEntity> standardCategoriesForUser, int userId)
    {
        if (!standardCategoriesForUser.Any(x => x.Name == StandardCategoryNames.InternalCashFlowName))
        {
            await context.Categories.AddAsync(new CategoryEntity
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
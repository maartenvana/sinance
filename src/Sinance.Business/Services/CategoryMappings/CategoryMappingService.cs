using Microsoft.EntityFrameworkCore;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.CategoryMapping;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.Services.CategoryMappings;

public class CategoryMappingService : ICategoryMappingService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly IUserIdProvider _userIdProvider;

    public CategoryMappingService(
        IDbContextFactory<SinanceContext> dbContextFactory,
        IUserIdProvider userIdProvider)
    {
        _dbContextFactory = dbContextFactory;
        _userIdProvider = userIdProvider;
    }

    public async Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(CategoryMappingModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var existingCategoryMapping = await context.CategoryMappings.SingleOrDefaultAsync(x =>
            x.ColumnTypeId == model.ColumnTypeId &&
            x.MatchValue == model.MatchValue &&
            x.CategoryId == model.CategoryId);

        if (existingCategoryMapping?.MatchValue.Equals(model.MatchValue, StringComparison.InvariantCultureIgnoreCase) == true)
            throw new AlreadyExistsException(nameof(CategoryMappingEntity));

        var entity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());

        await context.CategoryMappings.AddAsync(entity);

        await context.SaveChangesAsync();

        var insertedCategoryMapping = await FindCategoryMapping(entity.Id, context);

        return insertedCategoryMapping.ToDto();
    }

    public async Task DeleteCategoryMappingByIdForCurrentUser(int categoryMappingId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var mapping = await context.CategoryMappings.SingleOrDefaultAsync(item => item.Id == categoryMappingId);

        if (mapping == null)
            throw new NotFoundException(nameof(CategoryMappingEntity));

        context.CategoryMappings.Remove(mapping);
        await context.SaveChangesAsync();
    }

    public async Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int categoryMappingId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var entity = await FindCategoryMapping(categoryMappingId, context);

        if (entity == null)
            throw new NotFoundException(nameof(CategoryMappingEntity));

        return entity.ToDto();
    }

    public async Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(CategoryMappingModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var existingCategoryMapping = await FindCategoryMapping(model.Id, context);

        if (existingCategoryMapping == null)
            throw new NotFoundException(nameof(CategoryMappingEntity));

        existingCategoryMapping.UpdateEntityFromModel(model);
        await context.SaveChangesAsync();

        return existingCategoryMapping.ToDto();
    }

    private static async Task<CategoryMappingEntity> FindCategoryMapping(int categoryMappingId, SinanceContext context) =>
        await context.CategoryMappings.Include(x => x.Category).SingleOrDefaultAsync(item => item.Id == categoryMappingId);
}
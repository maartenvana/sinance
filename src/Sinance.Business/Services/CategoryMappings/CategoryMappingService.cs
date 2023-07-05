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
    private readonly Func<IUnitOfWork> _unitOfWork;
    private readonly IUserIdProvider _userIdProvider;

    public CategoryMappingService(
        Func<IUnitOfWork> unitOfWork,
        IUserIdProvider userIdProvider)
    {
        _unitOfWork = unitOfWork;
        _userIdProvider = userIdProvider;
    }

    public async Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(CategoryMappingModel model)
    {
        using var unitOfWork = _unitOfWork();

        var existingCategoryMapping = await unitOfWork.CategoryMappingRepository.FindSingle(findQuery: x =>
            x.ColumnTypeId == model.ColumnTypeId &&
            x.MatchValue == model.MatchValue &&
            x.CategoryId == model.CategoryId);

        if (existingCategoryMapping?.MatchValue.Equals(model.MatchValue, StringComparison.InvariantCultureIgnoreCase) == true)
        {
            throw new AlreadyExistsException(nameof(CategoryMappingEntity));
        }

        var entity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());

        unitOfWork.CategoryMappingRepository.Insert(entity);

        await unitOfWork.SaveAsync();

        var insertedCategoryMapping = await FindCategoryMapping(entity.Id, unitOfWork);

        return insertedCategoryMapping.ToDto();
    }

    public async Task DeleteCategoryMappingByIdForCurrentUser(int categoryMappingId)
    {
        using var unitOfWork = _unitOfWork();

        var mapping = await unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == categoryMappingId);

        if (mapping == null)
        {
            throw new NotFoundException(nameof(CategoryMappingEntity));
        }

        unitOfWork.CategoryMappingRepository.Delete(mapping);
        await unitOfWork.SaveAsync();
    }

    public async Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int categoryMappingId)
    {
        using var unitOfWork = _unitOfWork();

        var entity = await FindCategoryMapping(categoryMappingId, unitOfWork);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CategoryMappingEntity));
        }

        return entity.ToDto();
    }

    public async Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(CategoryMappingModel model)
    {
        using var unitOfWork = _unitOfWork();

        var existingCategoryMapping = await FindCategoryMapping(model.Id, unitOfWork);

        if (existingCategoryMapping == null)
        {
            throw new NotFoundException(nameof(CategoryMappingEntity));
        }

        existingCategoryMapping.UpdateEntityFromModel(model);
        await unitOfWork.SaveAsync();

        return existingCategoryMapping.ToDto();
    }

    private static async Task<CategoryMappingEntity> FindCategoryMapping(int categoryMappingId, IUnitOfWork unitOfWork)
    {
        return await unitOfWork.CategoryMappingRepository.FindSingleTracked(
            findQuery: item => item.Id == categoryMappingId,
            includeProperties: new string[] {
                nameof(CategoryMappingEntity.Category)
            });
    }
}
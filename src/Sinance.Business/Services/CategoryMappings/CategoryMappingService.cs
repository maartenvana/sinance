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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserIdProvider _userIdProvider;

    public CategoryMappingService(
        IUnitOfWork unitOfWork,
        IUserIdProvider userIdProvider)
    {
        _unitOfWork = unitOfWork;
        _userIdProvider = userIdProvider;
    }

    public async Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(CategoryMappingModel model)
    {
        

        var existingCategoryMapping = await _unitOfWork.CategoryMappingRepository.FindSingle(findQuery: x =>
            x.ColumnTypeId == model.ColumnTypeId &&
            x.MatchValue == model.MatchValue &&
            x.CategoryId == model.CategoryId);

        if (existingCategoryMapping?.MatchValue.Equals(model.MatchValue, StringComparison.InvariantCultureIgnoreCase) == true)
        {
            throw new AlreadyExistsException(nameof(CategoryMappingEntity));
        }

        var entity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());

        _unitOfWork.CategoryMappingRepository.Insert(entity);

        await _unitOfWork.SaveAsync();

        var insertedCategoryMapping = await FindCategoryMapping(entity.Id, _unitOfWork);

        return insertedCategoryMapping.ToDto();
    }

    public async Task DeleteCategoryMappingByIdForCurrentUser(int categoryMappingId)
    {
        

        var mapping = await _unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == categoryMappingId);

        if (mapping == null)
        {
            throw new NotFoundException(nameof(CategoryMappingEntity));
        }

        _unitOfWork.CategoryMappingRepository.Delete(mapping);
        await _unitOfWork.SaveAsync();
    }

    public async Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int categoryMappingId)
    {
        

        var entity = await FindCategoryMapping(categoryMappingId, _unitOfWork);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CategoryMappingEntity));
        }

        return entity.ToDto();
    }

    public async Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(CategoryMappingModel model)
    {
        

        var existingCategoryMapping = await FindCategoryMapping(model.Id, _unitOfWork);

        if (existingCategoryMapping == null)
        {
            throw new NotFoundException(nameof(CategoryMappingEntity));
        }

        existingCategoryMapping.UpdateEntityFromModel(model);
        await _unitOfWork.SaveAsync();

        return existingCategoryMapping.ToDto();
    }

    private static async Task<CategoryMappingEntity> FindCategoryMapping(int categoryMappingId, IUnitOfWork unitOfWork)
    {
        return await unitOfWork.CategoryMappingRepository.FindSingleTracked(
            findQuery: item => item.Id == categoryMappingId,
            nameof(CategoryMappingEntity.Category));
    }
}
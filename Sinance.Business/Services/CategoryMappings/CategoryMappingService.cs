using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.CategoryMapping;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.CategoryMappings
{
    public class CategoryMappingService : ICategoryMappingService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategoryMappingService(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(CategoryMappingModel model)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var existingCategoryMapping = await unitOfWork.CategoryMappingRepository.FindSingle(findQuery: x =>
                x.ColumnTypeId == model.ColumnTypeId &&
                x.MatchValue == model.MatchValue &&
                x.CategoryId == model.CategoryId &&
                x.UserId == userId);

            if (existingCategoryMapping?.MatchValue.Equals(model.MatchValue, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                throw new AlreadyExistsException(nameof(CategoryMappingEntity));
            }

            var entity = model.ToNewEntity(userId);

            unitOfWork.CategoryMappingRepository.Insert(entity);

            await unitOfWork.SaveAsync();

            var insertedCategoryMapping = await FindCategoryMapping(entity.Id, userId, unitOfWork);

            return insertedCategoryMapping.ToDto();
        }

        public async Task DeleteCategoryMappingByIdForCurrentUser(int categoryMappingId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var mapping = await unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == categoryMappingId &&
                               item.Category.UserId == userId);

            if (mapping == null)
            {
                throw new NotFoundException(nameof(CategoryMappingEntity));
            }

            unitOfWork.CategoryMappingRepository.Delete(mapping);
            await unitOfWork.SaveAsync();
        }

        public async Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int categoryMappingId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var entity = await FindCategoryMapping(categoryMappingId, userId, unitOfWork);

            if (entity == null)
            {
                throw new NotFoundException(nameof(CategoryMappingEntity));
            }

            return entity.ToDto();
        }

        public async Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(CategoryMappingModel model)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var existingCategoryMapping = await FindCategoryMapping(model.Id, userId, unitOfWork);

            if (existingCategoryMapping == null)
            {
                throw new NotFoundException(nameof(CategoryMappingEntity));
            }

            existingCategoryMapping.UpdateEntityFromModel(model);
            await unitOfWork.SaveAsync();

            return existingCategoryMapping.ToDto();
        }

        private static async Task<CategoryMappingEntity> FindCategoryMapping(int categoryMappingId, int userId, IUnitOfWork unitOfWork)
        {
            return await unitOfWork.CategoryMappingRepository.FindSingleTracked(
                findQuery: item =>
                    item.Id == categoryMappingId &&
                    item.Category.UserId == userId,
                includeProperties: new string[] {
                    nameof(CategoryMappingEntity.Category)
                });
        }
    }
}
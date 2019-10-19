using System;
using System.Threading.Tasks;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.CategoryMapping;
using Sinance.Storage;
using Sinance.Storage.Entities;

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

        public async Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(int userId, CategoryMappingModel model)
        {
            using var unitOfWork = _unitOfWork();

            var existingCategoryMapping = await unitOfWork.CategoryMappingRepository.FindSingle(findQuery: x =>
                x.ColumnTypeId == model.ColumnTypeId &&
                x.CategoryId == model.CategoryId &&
                x.UserId == userId);

            if (existingCategoryMapping?.MatchValue.Equals(model.MatchValue, StringComparison.InvariantCultureIgnoreCase) == true)
            {
                throw new AlreadyExistsException(nameof(CategoryMappingEntity));
            }

            var entity = model.ToNewEntity(userId);

            unitOfWork.CategoryMappingRepository.Insert(entity);

            await unitOfWork.SaveAsync();

            return entity.ToDto();
        }

        public async Task DeleteCategoryMappingByIdForCurrentUser(int userId, int categoryMappingId)
        {
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

        public async Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int userId, int categoryMappingId)
        {
            using var unitOfWork = _unitOfWork();

            var entity = await unitOfWork.CategoryMappingRepository.FindSingle(item =>
                item.Id == categoryMappingId && item.Category.UserId == userId);

            if (entity == null)
            {
                throw new NotFoundException(nameof(CategoryMappingEntity));
            }

            return entity.ToDto();
        }

        public async Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(int currentUserId, CategoryMappingModel model)
        {
            using var unitOfWork = _unitOfWork();

            var existingCategoryMapping = await unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == model.Id &&
                                                                                                                item.Category.UserId == currentUserId);
            if (existingCategoryMapping == null)
            {
                throw new NotFoundException(nameof(CategoryMappingEntity));
            }

            existingCategoryMapping.UpdateEntityFromModel(model);
            await unitOfWork.SaveAsync();

            return existingCategoryMapping.ToDto();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.CategoryMapping;
using Sinance.Storage;
using Sinance.Storage.Entities;

namespace Sinance.Business.Services.CategoryMappings
{
    public class CategoryMappingService : ICategoryMappingService
    {
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategoryMappingService(Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryMappingModel> CreateCategoryMapping(int userId, CategoryMappingModel model)
        {
            using var unitOfWork = _unitOfWork();

            await unitOfWork.SaveAsync();
        }

        public async Task DeleteCategoryMappingByIdForUser(int userId, int categoryMappingId)
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

        public async Task<CategoryMappingModel> GetCategoryMappingByIdForUser(int userId, int categoryMappingId)
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

        public async Task<CategoryMappingModel> UpdateCategoryMapping(int currentUserId, CategoryMappingModel model)
        {
            using var unitOfWork = _unitOfWork();

            var existingCategoryMapping = await unitOfWork.CategoryMappingRepository.FindSingleTracked(item => item.Id == model.Id &&
                                                                                                                item.Category.UserId == currentUserId);
            if (existingCategoryMapping != null)
            {
                existingCategoryMapping.Update();

                unitOfWork.CategoryMappingRepository.Update(existingCategoryMapping);
                await unitOfWork.SaveAsync();

                await unitOfWork.SaveAsync();
            }
        }
    }
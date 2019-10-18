using Sinance.Business.Extensions;
using Sinance.Communication.Model.Category;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategoryService(Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryModel>> GetAllCategoriesForUser(int userId)
        {
            using var unitOfWork = _unitOfWork();

            var allCategories = await unitOfWork.CategoryRepository.FindAll(item => item.UserId == userId);

            return allCategories.ToDto();
        }

        public async Task<IEnumerable<CategoryModel>> GetPossibleParentCategoriesForUser(int userId, int categoryId)
        {
            using var unitOfWork = _unitOfWork();

            var categories = await unitOfWork.CategoryRepository.FindAll(item => item.ParentId == null &&
                                                                        item.Id != categoryId &&
                                                                        item.UserId == userId);

            return categories.ToDto();
        }
    }
}
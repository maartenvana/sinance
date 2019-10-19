using Sinance.Communication.Model.Category;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryModel> CreateCategoryForUser(int userId, CategoryModel categoryModel);

        Task DeleteCategoryByIdForUser(int userId, int categoryId);

        Task<IEnumerable<CategoryModel>> GetAllCategoriesForUser(int userId);

        Task<CategoryModel> GetCategoryByIdForUser(int userId, int categoryId);

        Task<IEnumerable<CategoryModel>> GetPossibleParentCategoriesForUser(int userId, int categoryId);

        Task MapCategoryToTransactions(int userId, int categoryId, IEnumerable<int> transactionIds);

        Task<CategoryModel> UpdateCategoryForUser(int userId, CategoryModel categoryModel);
    }
}
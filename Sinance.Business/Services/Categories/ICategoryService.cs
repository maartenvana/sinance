using Sinance.Communication.Model.Category;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryModel> CreateCategoryForCurrentUser(CategoryModel categoryModel);

        Task DeleteCategoryByIdForCurrentUser(int categoryId);

        Task<IEnumerable<CategoryModel>> GetAllCategoriesForCurrentUser();

        Task<CategoryModel> GetCategoryByIdForCurrentUser(int categoryId);

        Task<IEnumerable<CategoryModel>> GetPossibleParentCategoriesForCurrentUser(int categoryId);

        Task MapCategoryToTransactionsForCurrentUser(int categoryId, IEnumerable<int> transactionIds);

        Task<CategoryModel> UpdateCategoryForCurrentUser(CategoryModel categoryModel);
    }
}
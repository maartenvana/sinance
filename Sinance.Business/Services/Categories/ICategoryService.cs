using Sinance.Communication.Model.Category;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryModel>> GetAllCategoriesForUser(int currentUserId);

        Task<IEnumerable<CategoryModel>> GetPossibleParentCategoriesForUser(int userId, int categoryId);
    }
}
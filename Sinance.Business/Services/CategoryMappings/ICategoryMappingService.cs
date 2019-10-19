using Sinance.Communication.CategoryMapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.CategoryMappings
{
    public interface ICategoryMappingService
    {
        Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(int userId, CategoryMappingModel model);

        Task DeleteCategoryMappingByIdForCurrentUser(int userId, int categoryMappingId);

        Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int userId, int categoryMappingId);

        Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(int currentUserId, CategoryMappingModel model);
    }
}
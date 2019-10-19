using Sinance.Communication.CategoryMapping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.CategoryMappings
{
    public interface ICategoryMappingService
    {
        Task<CategoryMappingModel> CreateCategoryMapping(int userId, CategoryMappingModel model);

        Task DeleteCategoryMappingByIdForUser(int userId, int categoryMappingId);

        Task<CategoryMappingModel> GetCategoryMappingByIdForUser(int userId, int categoryMappingId);

        Task<CategoryMappingModel> UpdateCategoryMapping(int currentUserId, CategoryMappingModel model);
    }
}
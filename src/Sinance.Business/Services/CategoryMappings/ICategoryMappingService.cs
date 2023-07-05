using Sinance.Communication.Model.CategoryMapping;
using System.Threading.Tasks;

namespace Sinance.Business.Services.CategoryMappings;

public interface ICategoryMappingService
{
    Task<CategoryMappingModel> CreateCategoryMappingForCurrentUser(CategoryMappingModel model);

    Task DeleteCategoryMappingByIdForCurrentUser(int categoryMappingId);

    Task<CategoryMappingModel> GetCategoryMappingByIdForCurrentUser(int categoryMappingId);

    Task<CategoryMappingModel> UpdateCategoryMappingForCurrentUser(CategoryMappingModel model);
}
using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Communication.Model.Import;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services;

public interface ICategoryService
{
    Task<CategoryModel> UpsertCategoryAsync(UpsertCategoryModel model);
    Task DeleteCategoryAsync(DeleteCategoryModel model);

    Task<List<TransactionModel>> AssignCategoryToTransactionsAsync(int? categoryId, List<TransactionModel> transactions);
    Task CreateAutoCategoryMappingAsync(int categoryId, ColumnType columnType, string columnValue);
    List<CategoryModel> GetAllCategories();
}
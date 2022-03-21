using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.BlazorApp.Business.Services
{
    public interface ICategoryService
    {
        List<TransactionModel> AssignCategoryToTransactions(int? categoryId, List<TransactionModel> transactions);
        List<CategoryModel> GetAllCategories();
    }
}
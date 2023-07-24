using Sinance.BlazorApp.Business.Model.Category;
using System.Collections.Generic;

namespace Sinance.BlazorApp.Business.Services;

public interface ICategoryService
{
    List<CategoryModel> GetAllCategories();
}
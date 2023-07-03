using Sinance.BlazorApp.Business.Model.Category;

namespace Sinance.BlazorApp.Extensions
{
    public static class CategoryModelExtensions
    {
        public static UpsertCategoryModel ToUpsertModel(this CategoryModel model) =>
            new()
            {
                Id = model.Id,
                Name = model.Name,
                ParentId = model.ParentId,
                ColorCode = model.ColorCode,
                ShortName = model.ShortName
            };
    }
}

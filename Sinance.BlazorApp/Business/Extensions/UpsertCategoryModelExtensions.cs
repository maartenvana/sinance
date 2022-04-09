using Sinance.BlazorApp.Business.Model.Category;
using Sinance.Storage.Entities;

namespace Sinance.BlazorApp.Business.Extensions
{
    public static class UpsertCategoryModelExtensions
    {
        public static CategoryEntity ToNewCategoryEntity(this UpsertCategoryModel model, int userId) => new()
        {
            Id = model.Id,
            Name = model.Name,
            ShortName = model.ShortName,
            ColorCode = model.ColorCode,
            ParentId = model.ParentId,
            UserId = userId
        };
    }
}

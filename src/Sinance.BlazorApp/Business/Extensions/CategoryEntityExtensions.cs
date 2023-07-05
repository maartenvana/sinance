using Sinance.BlazorApp.Business.Model.Category;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Extensions;

public static class CategoryEntityExtensions
{
    public static void Update(this CategoryEntity categoryEntity, UpsertCategoryModel model)
    {
        categoryEntity.ParentId = model.ParentId;
        categoryEntity.Name = model.Name;
        categoryEntity.ColorCode = model.ColorCode;
        categoryEntity.ParentId = model.ParentId;
        categoryEntity.ShortName = model.ShortName;
    }

    public static IEnumerable<CategoryModel> ToDto(this IEnumerable<CategoryEntity> entities)
        => entities.Select(x => x.ToDto());

    public static CategoryModel ToDto(this CategoryEntity entity)
    {
        return new CategoryModel
        {
            Id = entity.Id,
            Name = entity.Name,
            ShortName = entity.ShortName,
            ParentId = entity.ParentId,
            ColorCode = entity.ColorCode
        };
    }
}

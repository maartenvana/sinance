using Sinance.BlazorApp.Business.Model.Category;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Extensions
{
    public static class CategoryEntityExtensions
    {
        public static IEnumerable<CategoryModel> ToDto(this IEnumerable<CategoryEntity> entities)
            => entities.Select(x => x.ToDto());

        public static CategoryModel ToDto(this CategoryEntity entity)
        {
            return new CategoryModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentId = entity.ParentId,
                ColorCode = entity.ColorCode
            };
        }
    }
}

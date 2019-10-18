using Sinance.Communication.Model.Category;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinance.Business.Extensions
{
    public static class CategoryExtensions
    {
        public static IEnumerable<CategoryModel> ToDto(this IEnumerable<CategoryEntity> categoryEntity)
        {
            return categoryEntity.Select(x => x.ToDto());
        }

        public static CategoryModel ToDto(this CategoryEntity categoryEntity)
        {
            return new CategoryModel
            {
                ColorCode = categoryEntity.ColorCode,
                Id = categoryEntity.Id,
                IsRegular = categoryEntity.IsRegular,
                Name = categoryEntity.Name,
                ParentId = categoryEntity.ParentId
            };
        }
    }
}
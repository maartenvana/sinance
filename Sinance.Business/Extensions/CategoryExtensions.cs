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
        public static IEnumerable<CategoryModel> ToDto(this IEnumerable<CategoryEntity> categoryEntity) => categoryEntity.Select(x => x.ToDto());

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

        public static CategoryEntity ToNewEntity(this CategoryModel model, int userId)
        {
            return new CategoryEntity
            {
                Id = model.Id,
                ColorCode = model.ColorCode,
                ParentId = model.ParentId,
                IsRegular = model.IsRegular,
                Name = model.Name,
                UserId = userId
            };
        }

        public static CategoryEntity UpdateEntity(this CategoryEntity entity, CategoryModel model)
        {
            entity.ColorCode = model.ColorCode;
            entity.ParentId = model.ParentId;
            entity.IsRegular = model.IsRegular;
            entity.Name = model.Name;

            return entity;
        }
    }
}
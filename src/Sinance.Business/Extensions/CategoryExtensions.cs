using Sinance.Communication.Model.Category;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions
{
    public static class CategoryExtensions
    {
        public static List<CategoryModel> ToDto(this List<CategoryEntity> categoryEntity) => categoryEntity.Select(x => x.ToDto()).ToList();

        public static CategoryModel ToDto(this CategoryEntity categoryEntity)
        {
            var model = new CategoryModel
            {
                ColorCode = categoryEntity.ColorCode,
                Id = categoryEntity.Id,
                IsRegular = categoryEntity.IsRegular,
                Name = categoryEntity.Name,
                ParentId = categoryEntity.ParentId,
                HasChildren = categoryEntity.ChildCategories?.Any() == true,
                ParentCategoryIsRegular = categoryEntity.ParentCategory?.IsRegular == true
            };

            if (categoryEntity.CategoryMappings?.Any() == true)
            {
                model.Mappings = categoryEntity.CategoryMappings.ToDto();
            }

            return model;
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
            entity.ParentId = model.ParentId == 0 ? null : model.ParentId;
            entity.IsRegular = model.IsRegular;
            entity.Name = model.Name;

            return entity;
        }

        public static CategoryEntity UpdateStandardEntity(this CategoryEntity entity, CategoryModel model)
        {
            entity.ColorCode = model.ColorCode;
            entity.ParentId = model.ParentId == 0 ? null : model.ParentId;

            return entity;
        }
    }
}
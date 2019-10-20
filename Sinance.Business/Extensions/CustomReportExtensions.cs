using Sinance.Communication.Model.CustomReport;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions
{
    public static class CustomReportExtensions
    {
        public static IEnumerable<CustomReportModel> ToDto(this IEnumerable<CustomReportEntity> entities) => entities.Select(x => x.ToDto());

        public static CustomReportModel ToDto(this CustomReportEntity entity)
        {
            return new CustomReportModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Categories = entity.ReportCategories.Select(x => new CustomReportCategoryModel
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name
                }).ToList()
            };
        }

        public static CustomReportEntity ToNewEntity(this CustomReportModel model, int userId)
        {
            return new CustomReportEntity
            {
                Name = model.Name,
                UserId = userId,
                ReportCategories = model.Categories.Select(x => new CustomReportCategoryEntity
                {
                    CategoryId = x.CategoryId
                }).ToList()
            };
        }

        public static CustomReportEntity UpdateWithModel(this CustomReportEntity entity, CustomReportModel model)
        {
            entity.Name = model.Name;
            entity.ReportCategories = model.Categories.Select(x => new CustomReportCategoryEntity
            {
                CategoryId = x.CategoryId
            }).ToList();

            return entity;
        }
    }
}
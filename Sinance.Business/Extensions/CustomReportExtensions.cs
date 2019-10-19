using Sinance.Communication.Model.CustomReport;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                CustomReportCategories = entity.ReportCategories.Select(x => new CustomReportCategoryModel
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name
                }).ToList()
            };
        }
    }
}
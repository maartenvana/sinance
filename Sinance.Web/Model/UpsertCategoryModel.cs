using Microsoft.AspNetCore.Mvc.Rendering;
using Sinance.Communication.Model.Category;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class UpsertCategoryModel
    {
        public List<SelectListItem> AvailableParentCategories { get; set; } = new List<SelectListItem>();

        public CategoryModel CategoryModel { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc.Rendering;
using Sinance.Communication.Model.Category;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class UpsertCategoryModel
    {
        public IEnumerable<SelectListItem> AvailableParentCategories { get; set; }

        public CategoryModel CategoryModel { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class AddCategoryModel
    {
        public List<SelectListItem> AvailableCategories { get; set; }
    }
}
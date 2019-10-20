using Sinance.Communication.Model.Category;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class CategoriesOverviewViewModel
    {
        public List<CategoryModel> IrregularCategories { get; set; }

        public List<CategoryModel> RegularCategories { get; set; }
    }
}
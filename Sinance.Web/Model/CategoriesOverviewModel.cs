using Sinance.Domain.Entities;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class CategoriesOverviewModel
    {
        public List<Category> IrregularCategories { get; set; }
        public List<Category> RegularCategories { get; set; }
    }
}
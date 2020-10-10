using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Model.Category
{
    public class CategoryModel
    {
        public int Id { get; set; }

        public int? ParentId { get; set; }

        public string Name { get; set; }

        public string ColorCode { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Communication.Model.CustomReport
{
    public class CustomReportModel
    {
        public List<CustomReportCategoryModel> CustomReportCategories { get; set; } = new List<CustomReportCategoryModel>();

        public int Id { get; set; }

        public string Name { get; set; }
    }
}
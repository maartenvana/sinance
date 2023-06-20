using System.Collections.Generic;

namespace Sinance.Communication.Model.CustomReport;

public class CustomReportModel
{
    public List<CustomReportCategoryModel> Categories { get; set; } = new List<CustomReportCategoryModel>();

    public int Id { get; set; }

    public string Name { get; set; }
}
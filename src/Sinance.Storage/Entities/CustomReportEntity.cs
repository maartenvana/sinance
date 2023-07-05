using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Storage.Entities;

/// <summary>
/// Entity class for custom reports
/// </summary>
public class CustomReportEntity : UserEntityBase
{
    /// <summary>
    /// Name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Collection of categories to use in the report
    /// </summary>
    public List<CustomReportCategoryEntity> ReportCategories { get; set; }
}
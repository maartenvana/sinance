using Sinance.Communication.Model.CategoryMapping;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.Category;

public class CategoryModel
{
    /// <summary>
    /// Color code
    /// </summary>
    [Required]
    public string ColorCode { get; set; }

    /// <summary>
    /// If the category has child categories
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// Id of the category
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Is regular
    /// </summary>
    public bool IsRegular { get; set; }

    /// <summary>
    /// Automatic mapping configurations for the category
    /// </summary>
    public List<CategoryMappingModel> Mappings { get; set; } = new List<CategoryMappingModel>();

    /// <summary>
    /// Name
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; }

    /// <summary>
    /// If the (optional) parent category is a regular category
    /// </summary>
    public bool ParentCategoryIsRegular { get; set; }

    /// <summary>
    /// Parent id
    /// </summary>
    public int? ParentId { get; set; }
}
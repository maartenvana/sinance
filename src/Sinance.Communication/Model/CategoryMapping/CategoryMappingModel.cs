using Sinance.Communication.Model.Import;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Communication.Model.CategoryMapping;

/// <summary>
/// Category mapping model
/// </summary>
public class CategoryMappingModel
{
    /// <summary>
    /// Category id
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Category id
    /// </summary>
    public string CategoryName { get; set; }

    /// <summary>
    /// Column type id
    /// </summary>
    [Display(Name = "Kolom")]
    [Required(ErrorMessage = "{0} is vereist")]
    public ColumnType ColumnTypeId { get; set; }

    /// <summary>
    /// Id of the category mapping
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Match value
    /// </summary>
    [Display(Name = "Match waarde")]
    [Required(ErrorMessage = "{0} is vereist")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "{0} moet minimaal {2} en maximaal {1} karakters lang zijn")]
    public string MatchValue { get; set; }
}
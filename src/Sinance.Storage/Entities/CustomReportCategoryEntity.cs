using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// CustomReportCategory entity class
    /// </summary>
    public class CustomReportCategoryEntity : EntityBase
    {
        /// <summary>
        /// Category reference
        /// </summary>
        [ForeignKey("CategoryId")]
        public CategoryEntity Category { get; set; }

        /// <summary>
        /// Category identifier
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Custom report reference
        /// </summary>
        [ForeignKey("CustomReportId")]
        public CustomReportEntity CustomReport { get; set; }

        /// <summary>
        /// Custom report identifier
        /// </summary>
        [Required]
        public int CustomReportId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// CustomReportCategory entity class
    /// </summary>
    public class CustomReportCategory : UserEntityBase
    {
        /// <summary>
        /// Category reference
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Category identifier
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Custom report reference
        /// </summary>
        [ForeignKey("CustomReportId")]
        public virtual CustomReport CustomReport { get; set; }

        /// <summary>
        /// Custom report identifier
        /// </summary>
        [Required]
        public int CustomReportId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Transaction mapping entity
    /// </summary>
    public class ImportMapping : UserEntityBase
    {
        /// <summary>
        /// Column index
        /// </summary>
        [Required]
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Column name
        /// </summary>
        [Required]
        public string ColumnName { get; set; }

        /// <summary>
        /// Column type id
        /// </summary>
        [Required]
        public ColumnType ColumnTypeId { get; set; }

        /// <summary>
        /// Format value for formatting data or equalling data
        /// </summary>
        public string FormatValue { get; set; }

        /// <summary>
        /// Bank of mapping
        /// </summary>
        [ForeignKey("ImportBankId")]
        public virtual ImportBank ImportBank { get; set; }

        /// <summary>
        /// Id of bank
        /// </summary>
        [Required]
        public int ImportBankId { get; set; }
    }
}
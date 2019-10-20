using Sinance.Communication.Model.Import;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// Transaction mapping entity
    /// </summary>
    public class ImportMappingEntity : EntityBase
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
        public ImportBankEntity ImportBank { get; set; }

        /// <summary>
        /// Id of bank
        /// </summary>
        [Required]
        public int ImportBankId { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// Import bank record for configuration of an import for a bank
    /// </summary>
    public class ImportBankEntity : EntityBase
    {
        /// <summary>
        /// Delimiter for the import
        /// </summary>
        [Required]
        public string Delimiter { get; set; }

        /// <summary>
        /// If the import for this bank contains a header
        /// </summary>
        [Required]
        public bool ImportContainsHeader { get; set; }

        /// <summary>
        /// Import mappings for this bank
        /// </summary>
        public List<ImportMappingEntity> ImportMappings { get; set; }

        /// <summary>
        /// If this import bank definition is a standard available import
        /// </summary>
        public bool IsStandard { get; set; }

        /// <summary>
        /// Name of the bank
        /// </summary>
        [Required]
        public string Name { get; set; }
    }
}
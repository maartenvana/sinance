using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Import bank record for configuration of an import for a bank
    /// </summary>
    public class ImportBank : UserEntityBase
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
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed to be this")]
        public virtual ICollection<ImportMapping> ImportMappings { get; set; }

        /// <summary>
        /// Name of the bank
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ImportBank()
        {
            ImportMappings = new HashSet<ImportMapping>();
        }
    }
}
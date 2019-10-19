using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Storage.Entities
{
    /// <summary>
    /// Category entity
    /// </summary>
    public class CategoryEntity : UserEntityBase
    {
        /// <summary>
        /// Category mappings
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CategoryMappingEntity> CategoryMappings { get; set; }

        /// <summary>
        /// Child categories
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CategoryEntity> ChildCategories { get; set; }

        /// <summary>
        /// Color code
        /// </summary>
        [Required]
        public string ColorCode { get; set; }

        /// <summary>
        /// Is regular
        /// </summary>
        public bool IsRegular { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// ParentCategory
        /// </summary>
        [ForeignKey("ParentId")]
        public virtual CategoryEntity ParentCategory { get; set; }

        /// <summary>
        /// Parent id
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Transaction categories
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TransactionCategoryEntity> TransactionCategories { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// TODO: Check if it still is necesary
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "EF needs virtual collections for lazy loading")]
        public CategoryEntity()
        {
            ChildCategories = new HashSet<CategoryEntity>();
            TransactionCategories = new HashSet<TransactionCategoryEntity>();
            CategoryMappings = new HashSet<CategoryMappingEntity>();
        }
    }
}
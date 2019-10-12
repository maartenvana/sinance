using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Category entity
    /// </summary>
    public class Category : UserEntityBase
    {
        /// <summary>
        /// Category mappings
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CategoryMapping> CategoryMappings { get; set; }

        /// <summary>
        /// Child categories
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Category> ChildCategories { get; set; }

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
        public virtual Category ParentCategory { get; set; }

        /// <summary>
        /// Parent id
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Transaction categories
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TransactionCategory> TransactionCategories { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "EF needs virtual collections for lazy loading")]
        public Category()
        {
            ChildCategories = new HashSet<Category>();
            TransactionCategories = new HashSet<TransactionCategory>();
            CategoryMappings = new HashSet<CategoryMapping>();
        }

        /// <summary>
        /// Updates the current entity with the given info
        /// </summary>
        /// <param name="name">Name of the category</param>
        /// <param name="colorCode">color code</param>
        /// <param name="parentId">parent id</param>
        /// <param name="isRegular">Regular expense</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Update(string name, string colorCode, int? parentId, bool isRegular)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(colorCode))
                throw new ArgumentNullException(nameof(colorCode));

            Name = name;
            ColorCode = colorCode;
            ParentId = parentId;
            IsRegular = isRegular;
        }
    }
}
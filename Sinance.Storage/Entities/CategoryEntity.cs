using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public List<CategoryMappingEntity> CategoryMappings { get; set; }

        /// <summary>
        /// Child categories
        /// </summary>
        public List<CategoryEntity> ChildCategories { get; set; }

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
        public CategoryEntity ParentCategory { get; set; }

        /// <summary>
        /// Parent id
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Transaction categories
        /// </summary>
        public List<TransactionCategoryEntity> TransactionCategories { get; set; }
    }
}
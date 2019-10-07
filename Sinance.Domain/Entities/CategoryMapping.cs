using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Domain.Entities
{
    /// <summary>
    /// Category mapping entity
    /// </summary>
    public class CategoryMapping : UserEntityBase
    {
        /// <summary>
        /// Category
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Category id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Column type id
        /// </summary>
        [Display(Name = "Kolom")]
        [Required(ErrorMessage = "{0} is vereist")]
        public ColumnType ColumnTypeId { get; set; }

        /// <summary>
        /// Match value
        /// </summary>
        [Display(Name = "Match waarde")]
        [Required(ErrorMessage = "{0} is vereist")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "{0} moet minimaal {2} en maximaal {1} karakters lang zijn")]
        public string MatchValue { get; set; }

        /// <summary>
        /// Updates the current instance with the values from the given entity
        /// </summary>
        public void Update(string matchValue, ColumnType columnType, int categoryId)
        {
            if (matchValue == null)
                throw new ArgumentNullException(nameof(matchValue));

            MatchValue = matchValue;
            ColumnTypeId = columnType;
            CategoryId = categoryId;
        }
    }
}
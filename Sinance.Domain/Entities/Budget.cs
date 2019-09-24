using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Domain.Entities
{
    public class Budget : EntityBase
    {
        public decimal? Amount { get; set; }

        /// <summary>
        /// ParentCategory
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public int CategoryId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Domain.Entities
{
    public class UserEntityBase : EntityBase
    {
        /// <summary>
        /// User
        /// </summary>
        [ForeignKey("UserId")]
        public virtual SinanceUser User { get; set; }

        /// <summary>
        /// User id
        /// </summary>
        [Required]
        public int UserId { get; set; }
    }
}
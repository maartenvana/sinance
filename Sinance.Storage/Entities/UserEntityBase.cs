using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinance.Storage.Entities
{
    public class UserEntityBase : EntityBase
    {
        /// <summary>
        /// User
        /// </summary>
        [ForeignKey("UserId")]
        public virtual SinanceUserEntity User { get; set; }

        /// <summary>
        /// User id
        /// </summary>
        [Required]
        public int UserId { get; set; }
    }
}
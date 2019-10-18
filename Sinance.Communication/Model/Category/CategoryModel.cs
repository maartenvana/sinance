using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Sinance.Communication.Model.Category
{
    public class CategoryModel
    {
        /// <summary>
        /// Color code
        /// </summary>
        [Required]
        public string ColorCode { get; set; }

        /// <summary>
        /// Id of the category
        /// </summary>
        public int Id { get; set; }

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
        /// Parent id
        /// </summary>
        public int? ParentId { get; set; }
    }
}
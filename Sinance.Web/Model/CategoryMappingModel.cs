using Sinance.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Category mapping model
    /// </summary>
    public class CategoryMappingModel
    {
        /// <summary>
        /// Category id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Name of the category mapping
        /// </summary>
        [Display(Name = "Categorie")]
        public string CategoryName { get; set; }

        /// <summary>
        /// Column type id
        /// </summary>
        [Display(Name = "Kolom")]
        [Required(ErrorMessage = "{0} is vereist")]
        public ColumnType ColumnTypeId { get; set; }

        /// <summary>
        /// Id of the category mapping
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Match value
        /// </summary>
        [Display(Name = "Match waarde")]
        [Required(ErrorMessage = "{0} is vereist")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "{0} moet minimaal {2} en maximaal {1} karakters lang zijn")]
        public string MatchValue { get; set; }

        /// <summary>
        /// Creates a model with the given mapping
        /// </summary>
        /// <param name="mapping">Mapping to use</param>
        /// <returns>The created model</returns>
        public static CategoryMappingModel CreateCategoryMappingModel(CategoryMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            return new CategoryMappingModel
            {
                Id = mapping.Id,
                CategoryId = mapping.CategoryId,
                CategoryName = mapping.Category.Name,
                MatchValue = mapping.MatchValue,
                ColumnTypeId = mapping.ColumnTypeId
            };
        }
    }
}
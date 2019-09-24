using Sinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Category Model
    /// </summary>
    public class CategoryModel
    {
        /// <summary>
        /// Automatic mapped transaction to this category
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This will be resolved when category types are implemented")]
        public IEnumerable<KeyValuePair<Transaction, bool>> AutomaticMappings { get; set; }

        /// <summary>
        /// Available Categories
        /// </summary>
        public IEnumerable<SelectListItem> AvailableCategories { get; set; }

        /// <summary>
        /// Category mappings
        /// </summary>
        public IEnumerable<CategoryMapping> CategoryMappings { get; set; }

        /// <summary>
        /// Color code
        /// </summary>
        [Display(Name = "Kleur code")]
        [Required(ErrorMessage = "{0} is vereist")]
        public string ColorCode { get; set; }

        /// <summary>
        /// If this category has children categories
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// Id of the category
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Is regular
        /// </summary>
        [Display(Name = "Vaste last/inkomen")]
        public bool IsRegular { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Display(Name = "Naam")]
        [Required(ErrorMessage = "{0} is vereist")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Naam mag niet korter dan 3 karakter en niet langer dan 50 karakters zijn")]
        public string Name { get; set; }

        /// <summary>
        /// Parent category is regular
        /// </summary>
        public bool ParentCategoryIsRegular { get; set; }

        /// <summary>
        /// Parent id
        /// </summary>
        [Display(Name = "Hoofd categorie")]
        public int? ParentId { get; set; }

        /// <summary>
        /// Creates a new instance of the model with a category
        /// </summary>
        /// <param name="category">Category to use for filling properties</param>
        /// <returns>Created model</returns>
        public static CategoryModel CreateCategoryModel(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            return new CategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                ColorCode = category.ColorCode,
                IsRegular = category.IsRegular,
                ParentId = category.ParentId,
                ParentCategoryIsRegular = category.ParentCategory != null && category.ParentCategory.IsRegular,
                HasChildren = category.ChildCategories.Any(),
                CategoryMappings = category.CategoryMappings
            };
        }
    }
}
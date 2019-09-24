using Sinance.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model
{
    /// <summary>
    /// Model containing the parameters for building a report of expenses per category
    /// </summary>
    public class ExpensePerCategoryModel
    {
        /// <summary>
        /// Available categories to choose from
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<Category> AvailableCategories { get; set; }

        /// <summary>
        /// Currently selected category id
        /// </summary>
        [Display(Name = "Categorie")]
        public int SelectedCategoryId { get; set; }

        /// <summary>
        /// Currently selected month
        /// </summary>
        [Display(Name = "Maand")]
        public string SelectedMonth { get; set; }

        /// <summary>
        /// Currently selected year
        /// </summary>
        [Display(Name = "Jaar")]
        public string SelectedYear { get; set; }

        /// <summary>
        /// Time period to display
        /// </summary>
        public TimeFilter TimeFilter { get; set; }
    }
}
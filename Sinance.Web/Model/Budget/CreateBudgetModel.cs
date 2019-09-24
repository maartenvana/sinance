using Sinance.Domain.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model.Budget
{
    public class CreateBudgetModel
    {
        [Display(Name = "Budget")]
        public decimal Amount { get; set; }

        public List<Category> AvailableCategories { get; set; }

        [Display(Name = "Categorie")]
        public int SelectedCategoryId { get; set; }
    }
}
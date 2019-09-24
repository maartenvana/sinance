using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model.Budget
{
    public class BudgetModel
    {
        [Display(Name = "Budget")]
        public decimal Amount { get; set; }

        public int CategoryId { get; set; }

        [Display(Name = "Categorie")]
        public string CategoryName { get; set; }

        public int Id { get; set; }
        public int? ParentCategoryId { get; set; }

        [Display(Name = "Hoofd Categorie")]
        public string ParentCategoryName { get; set; }
    }
}
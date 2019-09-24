using System.ComponentModel.DataAnnotations;

namespace Sinance.Web.Model.Budget
{
    public class EditBudgetModel
    {
        [Display(Name = "Budget")]
        public decimal Amount { get; set; }

        public int BudgetId { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Sinance.Web.Model.Budget
{
    public class BudgetStatusModel
    {
        public Dictionary<BudgetModel, decimal> BudgetStatus { get; set; } = new Dictionary<BudgetModel, decimal>();
        public DateTime StatusDate { get; set; }
    }
}
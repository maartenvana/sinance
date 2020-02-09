using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IExpensePercentageCalculation
    {
        Task<IEnumerable<KeyValuePair<string, decimal>>> ExpensePercentagePerCategoryNameForMonth(DateTime startDate, DateTime endDate);
    }
}
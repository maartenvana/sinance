using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IExpenseCalculation
    {
        Task<Dictionary<string, IDictionary<int, decimal>>> ExpensePerCategoryIdPerMonthForYear(int year, IEnumerable<int> categoryIds);
    }
}
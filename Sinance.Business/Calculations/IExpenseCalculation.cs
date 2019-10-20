using Sinance.Communication.Model.StandardReport.Expense;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IExpenseCalculation
    {
        Task<BiMonthlyExpenseReportModel> BiMonthlyExpensePerCategoryReport(DateTime startMonth);

        Task<Dictionary<string, IDictionary<int, decimal>>> ExpensePerCategoryIdPerMonthForYear(int year, IEnumerable<int> categoryIds);
    }
}
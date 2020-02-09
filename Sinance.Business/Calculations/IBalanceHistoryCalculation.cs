using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IBalanceHistoryCalculation
    {
        Task<List<decimal[]>> BalanceHistoryForYear(int year, IEnumerable<int> includeBankAccounts);

        Task<List<decimal[]>> BalanceHistoryFromMonthsInPast(int monthsInPast, IEnumerable<int> includeBankAccounts);
    }
}
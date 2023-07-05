using Sinance.Communication.Model.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public interface IBalanceHistoryCalculation
{
    Task<List<BalanceHistoryRecord>> BalanceHistoryForYear(int year, IEnumerable<int> includeBankAccounts);

    Task<List<BalanceHistoryRecord>> BalanceHistoryForYearGroupedByType(int year, IEnumerable<int> includeBankAccounts);

    Task<List<BalanceHistoryRecord>> BalanceHistoryFromMonthsInPast(int monthsInPast, IEnumerable<int> includeBankAccounts);

    Task<List<BalanceHistoryRecord>> BalanceHistoryFromMonthsInPastGroupedByType(int monthsInPast, IEnumerable<int> includeBankAccounts);
}
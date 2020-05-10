using Sinance.Communication.Model.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IProfitLossCalculation
    {
        Task<IEnumerable<decimal>> CalculateProfitLosstPerMonthForYear(int year);
        Task<List<GroupedMonthlyProfitLossRecord>> CalculateProfitLosstPerMonthForYearGrouped(int year);
    }
}
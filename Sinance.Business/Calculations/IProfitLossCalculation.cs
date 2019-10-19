using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IProfitLossCalculation
    {
        Task<IEnumerable<decimal>> CalculateProfitLosstPerMonthForYear(int year);
    }
}
using Sinance.Communication.Model.StandardReport.Yearly;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IYearlyOverviewCalculation
    {
        Task<YearlyOverviewModel> CalculateForYear(int year);
    }
}
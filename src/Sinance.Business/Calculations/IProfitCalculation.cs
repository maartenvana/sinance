using Sinance.Communication.Model.Graph;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public interface IProfitCalculation
    {
        Task<List<MonthlyProfitLossRecord>> CalculateMonthlyProfit(DateTime startDate, DateTime endDate);

        Task<List<MonthlyProfitLossRecord>> CalculateMonthlyProfitGrouped(DateTime startDate, DateTime endDate);
    }
}
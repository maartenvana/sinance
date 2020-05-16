using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Calculations;
using Sinance.Communication.Model.BankAccount;
using Sinance.Web.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Web.ViewComponents
{
    public class ProfitGraphViewComponent : ViewComponent
    {
        private readonly IProfitCalculation profitLossCalculation;

        public ProfitGraphViewComponent(IProfitCalculation profitLossCalculation)
        {
            this.profitLossCalculation = profitLossCalculation;
        }

        public async Task<IViewComponentResult> InvokeAsync(DateTime startDate, DateTime endDate, bool grouped)
        {
            var profitPerMonth = grouped ? 
                await profitLossCalculation.CalculateMonthlyProfitGrouped(startDate, endDate) : 
                await profitLossCalculation.CalculateMonthlyProfit(startDate, endDate);

            var series = profitPerMonth.Select(x => new GraphSeriesEntry<decimal[]>
            {
                Name = grouped ? Enum.GetName(typeof(BankAccountType), x.AccountTypeGroup) : "Profit/loss",
                Data = x.ProfitPerMonth.AsEnumerable(),
                Type = "column"
            }).ToList();

            if (grouped)
            {
                series.Add(new GraphSeriesEntry<decimal[]>
                {
                    Name = "Total",
                    Data = profitPerMonth.SelectMany(x => x.ProfitPerMonth)
                                            .GroupBy(x => x[0])
                                            .Select(x => new decimal[] { x.Key, x.ToList().Sum(y => y[1]) }),
                    Type = "line"
                });
            }

            return View(series);
        }
    }
}
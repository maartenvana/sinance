using Sinance.Communication.Model.StandardReport.Income;
using System;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public interface IIncomeCalculation
{
    Task<BiMonthlyIncomeReportModel> BiMonthlyIncomePerCategoryReport(DateTime startMonth);
}
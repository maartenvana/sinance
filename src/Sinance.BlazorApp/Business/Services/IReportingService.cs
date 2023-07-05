using System;
using System.Collections.Generic;

namespace Sinance.BlazorApp.Business.Services;

public interface IReportingService
{
    Dictionary<DateTime, decimal> GetTotalPerMonthForCategory(int? categoryId, int yearStart, int yearEnd);
}
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ISinanceDbContextFactory<SinanceContext> dbContextFactory;

        public ReportingService(ISinanceDbContextFactory<SinanceContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public Dictionary<DateTime, decimal> GetTotalPerMonthForCategory(int? categoryId, int yearStart, int yearEnd)
        {
            var dateRangeStart = new DateTime(yearStart, 1, 1);
            var dateRangeEnd = new DateTime(yearEnd, 12, 31);

            using var context = dbContextFactory.CreateDbContext();

            return context.Transactions
                .Where(transaction =>
                    transaction.Date >= dateRangeStart &&
                    transaction.Date <= dateRangeEnd &&
                    transaction.CategoryId == categoryId)
                .OrderByDescending(transaction => transaction.Date)
                .Select(transaction => new { transaction.Date, transaction.Amount })
                .AsEnumerable()
                .GroupBy(x => new DateTime(x.Date.Year, x.Date.Month, 1))
                .ToDictionary(x => x.Key, x => x.Sum(x => x.Amount));
        }
    }
}

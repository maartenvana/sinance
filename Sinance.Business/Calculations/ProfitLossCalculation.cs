using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class ProfitLossCalculation : IProfitLossCalculation
    {
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ProfitLossCalculation(
            Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<decimal>> CalculateProfitLosstPerMonthForYear(int year)
        {
            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var transactionsPerMonth = (await unitOfWork.TransactionRepository
                .FindAll(item => item.Date.Year == year && item.BankAccount.IncludeInProfitLossGraph))
                .GroupBy(item => item.Date.Month)
                .ToList();

            var profitPerMonth = new List<decimal>();

            for (var month = 1; month <= 12; month++)
            {
                var transactions = transactionsPerMonth.SingleOrDefault(item => item.Key == month);
                profitPerMonth.Add(transactions?.Sum(item => item.Amount) ?? 0);
            }

            return profitPerMonth;
        }
    }
}
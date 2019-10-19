using Sinance.Business.Services.Authentication;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class ExpenseCalculation : IExpenseCalculation
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ExpenseCalculation(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<Dictionary<string, IDictionary<int, decimal>>> ExpensePerCategoryIdPerMonthForYear(int year, IEnumerable<int> categoryIds)
        {
            var dateRangeStart = new DateTime(year, 1, 1);
            var dateRangeEnd = new DateTime(year, 12, 31);

            using var unitOfWork = _unitOfWork();
            var userId = await _authenticationService.GetCurrentUserId();

            var transactions = await unitOfWork.TransactionRepository.FindAll(
                    findQuery: item =>
                        item.Date >= dateRangeStart &&
                        item.Date <= dateRangeEnd &&
                        item.UserId == userId &&
                        item.Amount < 0 &&
                        item.TransactionCategories.Any(transactionCategory =>
                            categoryIds.Any(reportCategory =>
                                reportCategory == transactionCategory.CategoryId)),
                    includeProperties: new string[] {
                        nameof(TransactionEntity.TransactionCategories),
                        $"{nameof(TransactionEntity.TransactionCategories)}.{nameof(TransactionCategoryEntity.Category)}"
                    });

            var reportDictionary = new Dictionary<string, IDictionary<int, decimal>>();

            foreach (var transaction in transactions)
            {
                if (transaction.TransactionCategories != null && transaction.TransactionCategories.Any())
                {
                    foreach (var transactionCategory in transaction.TransactionCategories.Where(transactionCategory => categoryIds.Any(reportCategory => reportCategory == transactionCategory.CategoryId)))
                    {
                        var category = transactionCategory.Category;

                        if (!reportDictionary.ContainsKey(category.Name))
                        {
                            reportDictionary.Add(category.Name, CreateMonthlyDictionary());
                        }

                        var amount = transactionCategory.Amount ?? transaction.Amount;
                        amount = amount < 0 ? amount * -1 : amount;

                        reportDictionary[category.Name][transaction.Date.Month] += amount;
                    }
                }
                else
                {
                    const string noCategoryName = "Geen categorie";
                    if (!reportDictionary.ContainsKey(noCategoryName))
                    {
                        reportDictionary.Add(noCategoryName, CreateMonthlyDictionary());
                    }

                    // Make sure the number is positive
                    reportDictionary[noCategoryName][transaction.Date.Month] += transaction.Amount < 0 ? transaction.Amount * -1 : transaction.Amount;
                }
            }

            return reportDictionary;
        }

        private static Dictionary<int, decimal> CreateMonthlyDictionary() =>
            new Dictionary<int, decimal>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 },
                { 6, 0 },
                { 7, 0 },
                { 8, 0 },
                { 9, 0 },
                { 10, 0 },
                { 11, 0 },
                { 12, 0 }
            };
    }
}
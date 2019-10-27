using Sinance.Business.Services.Authentication;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations
{
    public class ExpensePercentageCalculation : IExpensePercentageCalculation
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ExpensePercentageCalculation(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<IEnumerable<KeyValuePair<string, decimal>>> ExpensePercentagePerCategoryNameForMonth(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

            return await CalculateExpensePerCategoryName(startDate, endDate);
        }

        private async Task<IEnumerable<KeyValuePair<string, decimal>>> CalculateExpensePerCategoryName(DateTime startDate, DateTime endDate)
        {
            var currentUserId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var transactions = await unitOfWork.TransactionRepository
            .FindAll(
                findQuery: item =>
                    item.Date >= startDate &&
                    item.Date <= endDate &&
                    item.UserId == currentUserId &&
                    item.Amount < 0,
                includeProperties: new string[] {
                    nameof(TransactionEntity.TransactionCategories)
                });

            var categories = await unitOfWork.CategoryRepository.FindAll(x => x.UserId == currentUserId);

            var amountPerCategory = new Dictionary<int, decimal>(categories.Count + 1);
            var noneCategory = new CategoryEntity()
            {
                Id = 0,
                Name = "Geen"
            };

            foreach (var transaction in transactions)
            {
                if (transaction.TransactionCategories.Any())
                {
                    foreach (var transactionCategory in transaction.TransactionCategories.Where(item => item.CategoryId != 69))
                    {
                        var categoryId = transactionCategory.CategoryId;

                        if (!amountPerCategory.ContainsKey(categoryId))
                        {
                            amountPerCategory.Add(categoryId, 0M);
                        }

                        amountPerCategory[categoryId] += (transactionCategory.Amount ?? transaction.Amount) * -1;
                    }
                }
                else
                {
                    if (!amountPerCategory.ContainsKey(noneCategory.Id))
                    {
                        amountPerCategory.Add(noneCategory.Id, 0M);
                    }
                    amountPerCategory[noneCategory.Id] += transaction.Amount * -1;
                }
            }

            var total = amountPerCategory.Sum(x => x.Value);

            var percentagePerCategoryName = amountPerCategory.Select(x => new KeyValuePair<string, decimal>(
                key: categories.SingleOrDefault(cat => cat.Id == x.Key)?.Name ?? noneCategory.Name,
                value: (x.Value / total) * 100));

            return percentagePerCategoryName;
        }
    }
}
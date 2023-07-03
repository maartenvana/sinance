using Sinance.Business.Constants;
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
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ExpensePercentageCalculation(Func<IUnitOfWork> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<KeyValuePair<string, decimal>>> ExpensePercentagePerCategoryNameForMonth(DateTime startDate, DateTime endDate)
        {
            using var unitOfWork = _unitOfWork();

            // No need to sort this list, we loop through it by month numbers
            var transactions = await unitOfWork.TransactionRepository.FindAll(findQuery: item =>
                item.Date >= startDate &&
                item.Date <= endDate &&
                item.Amount < 0);

            var categories = await unitOfWork.CategoryRepository.ListAll();

            var internalCashFlowCategory = categories.Single(x => x.Name == StandardCategoryNames.InternalCashFlowName);

            var amountPerCategory = new Dictionary<int, decimal>(categories.Count + 1);
            var noneCategory = new CategoryEntity()
            {
                Id = 0,
                Name = "Geen"
            };

            foreach (var transaction in transactions)
            {
                if (transaction.CategoryId.HasValue && transaction.CategoryId != internalCashFlowCategory.Id)
                {
                    var categoryId = transaction.CategoryId.Value;
                    if (!amountPerCategory.ContainsKey(categoryId))
                    {
                        amountPerCategory.Add(categoryId, 0M);
                    }

                    amountPerCategory[categoryId] += transaction.Amount * -1;
                }
                else if (transaction.CategoryId != internalCashFlowCategory.Id)
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
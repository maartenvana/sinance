using Microsoft.EntityFrameworkCore;
using Sinance.Business.Constants;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public class ExpensePercentageCalculation : IExpensePercentageCalculation
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public ExpensePercentageCalculation(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<KeyValuePair<string, decimal>>> ExpensePercentagePerCategoryNameForMonth(DateTime startDate, DateTime endDate)
    {
        using var context = _dbContextFactory.CreateDbContext();

        // No need to sort this list, we loop through it by month numbers
        var transactions = await context.Transactions
            .Include(x => x.TransactionCategories)
            .Where(item => item.Date >= startDate && item.Date <= endDate && item.Amount < 0)
            .ToListAsync();

        var categories = await context.Categories.ToListAsync();

        var internalCashFlowCategory = categories.Single(x => x.Name == StandardCategoryNames.InternalCashFlowName);

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
                foreach (var transactionCategory in transaction.TransactionCategories.Where(item => item.CategoryId != internalCashFlowCategory.Id))
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
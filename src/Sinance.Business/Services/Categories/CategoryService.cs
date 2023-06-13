using Microsoft.EntityFrameworkCore;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.Category;
using Sinance.Communication.Model.Import;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly IUserIdProvider _userIdProvider;

    public CategoryService(
        IDbContextFactory<SinanceContext> dbContextFactory,
        IUserIdProvider userIdProvider)
    {
        _dbContextFactory = dbContextFactory;
        _userIdProvider = userIdProvider;
    }

    public async Task<CategoryModel> CreateCategoryForCurrentUser(CategoryModel categoryModel)
    {
        using var context = _dbContextFactory.CreateDbContext();
        
        var category = await context.Categories.SingleOrDefaultAsync(item => item.Name == categoryModel.Name);

        if (category != null)
            throw new AlreadyExistsException(nameof(CategoryEntity));

        var newCategory = categoryModel.ToNewEntity(_userIdProvider.GetCurrentUserId());

        await context.Categories.AddAsync(newCategory);
        await context.SaveChangesAsync();

        return newCategory.ToDto();
    }

    public async Task<List<KeyValuePair<TransactionModel, bool>>> CreateCategoryMappingToTransactionsForUser(CategoryModel category)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var allTransactions = await context.Transactions.ToListAsync();

        var categoryMappings = await context.CategoryMappings.Where(item => item.CategoryId == category.Id).ToListAsync();

        var mappedTransactions = MapTransactionsWithCategoryMappings(categoryMappings, allTransactions)
            .Select(item => new KeyValuePair<TransactionModel, bool>(item.ToDto(), true))
            .ToList();

        return mappedTransactions;
    }

    public async Task DeleteCategoryByIdForCurrentUser(int categoryId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var category = await context.Categories.SingleOrDefaultAsync(item => item.Id == categoryId);
        if (category == null)
            throw new NotFoundException(nameof(CategoryEntity));
        else if (category.IsStandard)
            throw new DeleteStandardCategoryException();

        var transactionCategories = await context.TransactionCategories.Where(x => x.CategoryId == categoryId).ToListAsync();

        context.TransactionCategories.RemoveRange(transactionCategories);
        context.Categories.Remove(category);

        await context.SaveChangesAsync();
    }

    public async Task<List<CategoryModel>> GetAllCategoriesForCurrentUser()
    {
        using var context = _dbContextFactory.CreateDbContext();

        var allCategories = await context.Categories.ToListAsync();

        return allCategories.ToDto().ToList();
    }

    public async Task<CategoryModel> GetCategoryByIdForCurrentUser(int categoryId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var category = await context.Categories
            .Include(x => x.ParentCategory)
            .Include(x => x.ChildCategories)
            .Include(x => x.CategoryMappings)
            .SingleOrDefaultAsync(item => item.Id == categoryId);

        if (category == null)
            throw new NotFoundException(nameof(CategoryEntity));

        return category.ToDto();
    }

    public async Task<List<CategoryModel>> GetPossibleParentCategoriesForCurrentUser(int categoryId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var categories = await context.Categories
            .Where(item => item.ParentId == null && item.Id != categoryId)
            .ToListAsync();

        return categories.ToDto().ToList();
    }

    public async Task MapCategoryToTransactionsForCurrentUser(int categoryId, IEnumerable<int> transactionIds)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var category = await context.Categories.SingleOrDefaultAsync(x => x.Id == categoryId);
        if (category == null)
            throw new NotFoundException(nameof(CategoryEntity));

        var transactions = await context.Transactions
            .Include(x => x.TransactionCategories)
            .Where(x => transactionIds.Any(y => y == x.Id))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.TransactionCategories = new List<TransactionCategoryEntity>
            {
                new TransactionCategoryEntity
                {
                    TransactionId = transaction.Id,
                    Amount = null,
                    CategoryId = category.Id
                }
            };
        }

        await context.SaveChangesAsync();
    }

    public async Task<CategoryModel> UpdateCategoryForCurrentUser(CategoryModel categoryModel)
    {
        using var context = _dbContextFactory.CreateDbContext();
        
        var category = await context.Categories.SingleOrDefaultAsync(x => x.Id == categoryModel.Id);

        if (category == null)
            throw new NotFoundException(nameof(CategoryEntity));

        if (category.IsStandard)
            category.UpdateStandardEntity(categoryModel);
        else
            category.UpdateEntity(categoryModel);

        await context.SaveChangesAsync();

        return category.ToDto();
    }

    private static bool FieldContains(string transactionValue, string matchValue) => transactionValue?.Contains(matchValue, StringComparison.InvariantCultureIgnoreCase) == true;

    private static IEnumerable<TransactionEntity> MapTransactionsWithCategoryMappings(IEnumerable<CategoryMappingEntity> categoryMappings, IEnumerable<TransactionEntity> transactions)
    {
        foreach (var transaction in transactions)
        {
            foreach (var mapping in categoryMappings)
            {
                var isMatch = false;

                switch (mapping.ColumnTypeId)
                {
                    case ColumnType.Description:
                        isMatch = FieldContains(transaction.Description, mapping.MatchValue);
                        break;

                    case ColumnType.Name:
                        isMatch = FieldContains(transaction.Name, mapping.MatchValue);
                        break;

                    case ColumnType.DestinationAccount:
                        isMatch = FieldContains(transaction.DestinationAccount, mapping.MatchValue);
                        break;

                    default:
                        break;
                }

                if (isMatch)
                {
                    yield return transaction;
                    break;
                }
            }
        }
    }
}
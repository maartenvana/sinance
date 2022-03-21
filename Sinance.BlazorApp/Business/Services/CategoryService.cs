using Sinance.BlazorApp.Business.Exceptions;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<SinanceContext> dbContextFactory;

        public CategoryService(IDbContextFactory<SinanceContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public List<TransactionModel> AssignCategoryToTransactions(int? categoryId, List<TransactionModel> transactions)
        {
            using var context = dbContextFactory.CreateDbContext();

            var transactionIds = transactions.Select(x => x.Id);

            var transactionEntities = context.Transactions.Where(x => transactionIds.Contains(x.Id));

            ValidateCategoryExists(categoryId, context);

            foreach (var transactionEntity in transactionEntities)
            {
                transactionEntity.CategoryId = categoryId;
            }

            context.SaveChanges();

            return transactionEntities.ToDto().ToList();
        }

        private static void ValidateCategoryExists(int? categoryId, SinanceContext context)
        {
            if (categoryId != null)
            {
                var categoryEntity = context.Categories.SingleOrDefault(x => x.Id == categoryId);

                if (categoryEntity == null)
                    throw new NotFoundException($"No category for id {categoryId} found");
            }
        }

        public List<CategoryModel> GetAllCategories()
        {
            using var context = dbContextFactory.CreateDbContext();

            var bankAccountEntities = context.Categories.ToList();

            return bankAccountEntities.ToDto().ToList();
        }
    }
}
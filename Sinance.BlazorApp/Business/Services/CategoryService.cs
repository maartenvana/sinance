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

        public List<TransactionModel> AssignCategoryToTransactions(CategoryModel category, List<TransactionModel> transactions)
        {
            using var context = this.dbContextFactory.CreateDbContext();

            var transactionIds = transactions.Select(x => x.Id);

            var transactionEntities = context.Transactions.Where(x => transactionIds.Contains(x.Id));
            var categoryEntity = context.Categories.Single(x => x.Id == category.Id);

            foreach (var transactionEntity in transactionEntities)
            {
                transactionEntity.CategoryId = category.Id;
            }

            context.SaveChanges();

            return transactionEntities.ToDto().ToList();
        }

        public List<CategoryModel> GetAllCategories()
        {
            using var context = this.dbContextFactory.CreateDbContext();

            var bankAccountEntities = context.Categories.ToList();

            return bankAccountEntities.ToDto().ToList();
        }
    }
}
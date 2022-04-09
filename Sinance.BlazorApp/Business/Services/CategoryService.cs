using Sinance.BlazorApp.Business.Exceptions;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.Category;
using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.BlazorApp.Storage;
using Sinance.Communication.Model.Import;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ISinanceDbContextFactory<SinanceContext> dbContextFactory;
        private readonly IUserIdProvider userIdProvider;

        public CategoryService(
            ISinanceDbContextFactory<SinanceContext> dbContextFactory,
            IUserIdProvider userIdProvider)
        {
            this.dbContextFactory = dbContextFactory;
            this.userIdProvider = userIdProvider;
        }

        public async Task CreateAutoCategoryMappingAsync(int categoryId, ColumnType columnType, string columnValue)
        {
            using var context = dbContextFactory.CreateDbContext();

            var mappingEntity = new CategoryMappingEntity
            {
                CategoryId = categoryId,
                ColumnTypeId = columnType,
                MatchValue = columnValue,
                UserId = userIdProvider.GetCurrentUserId()
            };

            context.CategoryMappings.Add(mappingEntity);

            await context.SaveChangesAsync();
        }

        public async Task<List<TransactionModel>> AssignCategoryToTransactionsAsync(int? categoryId, List<TransactionModel> transactions)
        {
            using var context = dbContextFactory.CreateDbContext();

            var transactionIds = transactions.Select(x => x.Id);

            var transactionEntities = context.Transactions.Where(x => transactionIds.Contains(x.Id));

            ValidateCategoryExists(categoryId, context);

            foreach (var transactionEntity in transactionEntities)
            {
                transactionEntity.CategoryId = categoryId;
            }

            await context.SaveChangesAsync();

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

            var categories = context.Categories.ToList();

            return categories.ToDto().ToList();
        }

        public async Task<CategoryModel> UpsertCategoryAsync(UpsertCategoryModel model)
        {
            using var context = dbContextFactory.CreateDbContext();

            CategoryEntity categoryEntity;

            if (model.IsNew)
            {
                categoryEntity = model.ToNewCategoryEntity(userIdProvider.GetCurrentUserId());
                context.Categories.Add(categoryEntity);
            }
            else
            {
                categoryEntity = context.Categories.Single(x => x.Id == model.Id);
                categoryEntity.Update(model);
            }

            await context.SaveChangesAsync();

            return categoryEntity.ToDto();
        }

        public async Task DeleteCategoryAsync(DeleteCategoryModel model)
        {
            using var context = dbContextFactory.CreateDbContext();

            var categoryToDelete = context.Categories.Single(x => x.Id == model.CategoryId);

            context.Categories.Remove(categoryToDelete);

            await context.SaveChangesAsync();
        }
    }
}
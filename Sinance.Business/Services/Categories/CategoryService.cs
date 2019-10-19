using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Import;
using Sinance.Communication.Model.Category;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public CategoryService(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<CategoryModel> CreateCategoryForUser(int userId, CategoryModel categoryModel)
        {
            using var unitOfWork = _unitOfWork();
            var category = await unitOfWork.CategoryRepository.FindSingleTracked(item => item.Name == categoryModel.Name && item.UserId == userId);

            if (category != null)
            {
                throw new AlreadyExistsException(nameof(CategoryEntity));
            }

            var newCategory = categoryModel.ToNewEntity(userId);

            unitOfWork.CategoryRepository.Insert(newCategory);
            await unitOfWork.SaveAsync();

            return newCategory.ToDto();
        }

        public async Task<IEnumerable<KeyValuePair<TransactionModel, bool>>> CreateCategoryMappingToTransactionsForUser(int userId, CategoryModel category)
        {
            using var unitOfWork = _unitOfWork();

            var allTransactions = await unitOfWork.TransactionRepository.FindAll(item =>
                item.UserId == userId);

            var categoryMappings = await unitOfWork.CategoryMappingRepository.FindAll(item => item.UserId == userId && item.CategoryId == category.Id);

            var mappedTransactions = MapTransactionsWithCategoryMappings(categoryMappings, allTransactions)
                .Select(item => new KeyValuePair<TransactionModel, bool>(item.ToDto(), true));

            return mappedTransactions;
        }

        public async Task DeleteCategoryByIdForUser(int userId, int categoryId)
        {
            using var unitOfWork = _unitOfWork();

            var category = await unitOfWork.CategoryRepository.FindSingleTracked(item => item.Id == categoryId && item.UserId == userId);
            if (category == null)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }

            var transactionCategories = await unitOfWork.TransactionCategoryRepository.FindAllTracked(x => x.CategoryId == categoryId);

            unitOfWork.TransactionCategoryRepository.DeleteRange(transactionCategories);
            unitOfWork.CategoryRepository.Delete(category);

            await unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<CategoryModel>> GetAllCategoriesForUser(int userId)
        {
            using var unitOfWork = _unitOfWork();

            var allCategories = await unitOfWork.CategoryRepository.FindAll(item => item.UserId == userId);

            return allCategories.ToDto();
        }

        public async Task<CategoryModel> GetCategoryByIdForUser(int userId, int categoryId)
        {
            using var unitOfWork = _unitOfWork();

            var category = await unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId &&
                   item.UserId == userId,
                   includeProperties: new string[] {
                       nameof(CategoryEntity.ParentCategory),
                       nameof(CategoryEntity.ChildCategories),
                       nameof(CategoryEntity.CategoryMappings)
                   });

            if (category == null)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }

            return category.ToDto();
        }

        public async Task<IEnumerable<CategoryModel>> GetPossibleParentCategoriesForUser(int userId, int categoryId)
        {
            using var unitOfWork = _unitOfWork();

            var categories = await unitOfWork.CategoryRepository.FindAll(item => item.ParentId == null &&
                                                                        item.Id != categoryId &&
                                                                        item.UserId == userId);

            return categories.ToDto();
        }

        public async Task MapCategoryToTransactions(int userId, int categoryId, IEnumerable<int> transactionIds)
        {
            using var unitOfWork = _unitOfWork();

            var category = await unitOfWork.CategoryRepository.FindSingle(x => x.Id == categoryId);
            if (category == null)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }

            var transactions = await unitOfWork.TransactionRepository.FindAllTracked(
                x => x.UserId == userId && transactionIds.Any(y => y == x.Id),
                includeProperties: nameof(TransactionEntity.TransactionCategories));

            foreach (var transaction in transactions)
            {
                transaction.TransactionCategories = new List<TransactionCategory>
                {
                    new TransactionCategory
                    {
                        TransactionId = transaction.Id,
                        Amount = null,
                        CategoryId = category.Id
                    }
                };
            }

            await unitOfWork.SaveAsync();
        }

        public async Task<CategoryModel> UpdateCategoryForUser(int userId, CategoryModel categoryModel)
        {
            using var unitOfWork = _unitOfWork();

            var category = await unitOfWork.CategoryRepository.FindSingleTracked(x => x.Id == categoryModel.Id && x.UserId == userId);

            if (category == null)
            {
                throw new NotFoundException(nameof(CategoryEntity));
            }

            category.UpdateEntity(categoryModel);

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
                            isMatch = FieldContains(transaction.Name, mapping.MatchValue); ;
                            break;

                        case ColumnType.DestinationAccount:
                            isMatch = FieldContains(transaction.DestinationAccount, mapping.MatchValue); ;
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
}
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserIdProvider _userIdProvider;

    public CategoryService(
        IUnitOfWork unitOfWork,
        IUserIdProvider userIdProvider)
    {
        _unitOfWork = unitOfWork;
        _userIdProvider = userIdProvider;
    }

    public async Task<CategoryModel> CreateCategoryForCurrentUser(CategoryModel categoryModel)
    {
        
        var category = await _unitOfWork.CategoryRepository.FindSingleTracked(item => item.Name == categoryModel.Name);

        if (category != null)
        {
            throw new AlreadyExistsException(nameof(CategoryEntity));
        }

        var newCategory = categoryModel.ToNewEntity(_userIdProvider.GetCurrentUserId());

        _unitOfWork.CategoryRepository.Insert(newCategory);
        await _unitOfWork.SaveAsync();

        return newCategory.ToDto();
    }

    public async Task<List<KeyValuePair<TransactionModel, bool>>> CreateCategoryMappingToTransactionsForUser(CategoryModel category)
    {
        

        var allTransactions = await _unitOfWork.TransactionRepository.ListAll();

        var categoryMappings = await _unitOfWork.CategoryMappingRepository.FindAll(item => item.CategoryId == category.Id);

        var mappedTransactions = MapTransactionsWithCategoryMappings(categoryMappings, allTransactions)
            .Select(item => new KeyValuePair<TransactionModel, bool>(item.ToDto(), true))
            .ToList();

        return mappedTransactions;
    }

    public async Task DeleteCategoryByIdForCurrentUser(int categoryId)
    {
        

        var category = await _unitOfWork.CategoryRepository.FindSingleTracked(item => item.Id == categoryId);
        if (category == null)
        {
            throw new NotFoundException(nameof(CategoryEntity));
        }
        else if (category.IsStandard)
        {
            throw new DeleteStandardCategoryException();
        }

        var transactionCategories = await _unitOfWork.TransactionCategoryRepository.FindAllTracked(x => x.CategoryId == categoryId);

        _unitOfWork.TransactionCategoryRepository.DeleteRange(transactionCategories);
        _unitOfWork.CategoryRepository.Delete(category);

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<CategoryModel>> GetAllCategoriesForCurrentUser()
    {
        

        var allCategories = await _unitOfWork.CategoryRepository.ListAll();

        return allCategories.ToDto().ToList();
    }

    public async Task<CategoryModel> GetCategoryByIdForCurrentUser(int categoryId)
    {
        

        var category = await _unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId,
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

    public async Task<List<CategoryModel>> GetPossibleParentCategoriesForCurrentUser(int categoryId)
    {
        

        var categories = await _unitOfWork.CategoryRepository.FindAll(item => item.ParentId == null &&
                                                                    item.Id != categoryId);

        return categories.ToDto().ToList();
    }

    public async Task MapCategoryToTransactionsForCurrentUser(int categoryId, IEnumerable<int> transactionIds)
    {
        

        var category = await _unitOfWork.CategoryRepository.FindSingle(x => x.Id == categoryId);
        if (category == null)
        {
            throw new NotFoundException(nameof(CategoryEntity));
        }

        var transactions = await _unitOfWork.TransactionRepository.FindAllTracked(
            x => transactionIds.Any(y => y == x.Id),
            includeProperties: nameof(TransactionEntity.TransactionCategories));

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

        await _unitOfWork.SaveAsync();
    }

    public async Task<CategoryModel> UpdateCategoryForCurrentUser(CategoryModel categoryModel)
    {
        

        var category = await _unitOfWork.CategoryRepository.FindSingleTracked(x => x.Id == categoryModel.Id);

        if (category == null)
        {
            throw new NotFoundException(nameof(CategoryEntity));
        }

        if (category.IsStandard)
        {
            category.UpdateStandardEntity(categoryModel);
        }
        else
        {
            category.UpdateEntity(categoryModel);
        }

        await _unitOfWork.SaveAsync();

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
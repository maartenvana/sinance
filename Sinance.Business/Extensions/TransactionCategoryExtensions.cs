﻿using Sinance.Business.Exceptions;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions
{
    public static class TransactionCategoryExtensions
    {
        public static List<TransactionCategoryModel> ToDto(this List<TransactionCategoryEntity> transactionCategory) => transactionCategory.Select(x => x.ToDto()).ToList();

        public static TransactionCategoryModel ToDto(this TransactionCategoryEntity transactionCategory)
        {
            if (transactionCategory.Category == null)
            {
                throw new ModelConversionException("Category is null, cannot convert");
            }

            return new TransactionCategoryModel
            {
                Amount = transactionCategory.Amount,
                CategoryId = transactionCategory.CategoryId,
                Name = transactionCategory.Category.Name,
                ColorCode = transactionCategory.Category.ColorCode
            };
        }
    }
}
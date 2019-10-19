using Sinance.Communication.Model.Transaction;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions
{
    public static class TransactionExtensions
    {
        public static IEnumerable<TransactionModel> ToDto(this IEnumerable<TransactionEntity> transactions) => transactions.Select(x => x.ToDto());

        public static TransactionModel ToDto(this TransactionEntity transaction)
        {
            return new TransactionModel
            {
                Id = transaction.Id,
                Name = transaction.Name,
                BankAccountId = transaction.BankAccountId,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Categories = transaction.TransactionCategories != null ? transaction.TransactionCategories.ToDto().ToList() : new List<TransactionCategoryModel>(),
                Description = transaction.Description,
                DestinationAccount = transaction.DestinationAccount
            };
        }

        public static TransactionEntity ToNewEntity(this TransactionModel transactionModel, int userId)
        {
            return new TransactionEntity
            {
                Name = transactionModel.Name,
                Description = transactionModel.Description,
                DestinationAccount = transactionModel.DestinationAccount,
                Amount = transactionModel.Amount,
                Date = transactionModel.Date,
                BankAccountId = transactionModel.BankAccountId,
                UserId = userId
            };
        }

        public static TransactionEntity UpdateFromModel(this TransactionEntity transactionEntity, TransactionModel model)
        {
            transactionEntity.Name = model.Name;
            transactionEntity.Description = model.Description;
            transactionEntity.DestinationAccount = model.DestinationAccount;
            transactionEntity.Amount = model.Amount;
            transactionEntity.Date = model.Date;
            transactionEntity.BankAccountId = model.BankAccountId;

            return transactionEntity;
        }
    }
}
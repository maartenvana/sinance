using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Extensions
{
    public static class TransactionEntityExtensions
    {
        public static IEnumerable<TransactionModel> ToDto(this IEnumerable<TransactionEntity> entities)
            => entities.Select(x => x.ToDto());

        public static IEnumerable<TransactionEntity> SplitToNewTransactions(
            this TransactionEntity transactionEntity,
            IEnumerable<CreateTransactionModel> newTransactions) =>
            newTransactions.Select(x => transactionEntity.ToNewSplittedEntity(x));

        public static TransactionEntity ToNewSplittedEntity(
            this TransactionEntity transactionEntity,
            CreateTransactionModel model) =>
            new()
            {
                Name = model.Name,
                Date = model.Date,
                Amount = model.Amount,
                CategoryId = model.CategoryId,
                Description = model.Description,
                AccountNumber = model.SourceAccountNumber,
                DestinationAccount = model.DestinationAccountNumber,

                UserId = transactionEntity.UserId,
                BankAccountId = transactionEntity.BankAccountId,
                ImportTransactionId = transactionEntity.ImportTransactionId
            };

        public static TransactionModel ToDto(this TransactionEntity entity) => 
            new()
            {
                Id = entity.Id,
                Name = entity.Name,
                Date = entity.Date,
                Amount = entity.Amount,
                Description = entity.Description,
                SourceAccountNumber = entity.AccountNumber,
                DestinationAccountNumber = entity.DestinationAccount,

                CategoryId = entity.Category?.Id,
                CategoryColorCode = entity.Category?.ColorCode,
                CategoryShortName = entity.Category?.ShortName
            };
    }
}

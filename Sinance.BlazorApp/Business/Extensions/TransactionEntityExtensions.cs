using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Storage.Entities;

namespace Sinance.BlazorApp.Business.Extensions
{
    public static class TransactionEntityExtensions
    {
        public static IEnumerable<TransactionEntity> SplitToNewTransactions(
            this TransactionEntity transactionEntity,
            IEnumerable<UpsertTransactionModel> newTransactions) =>
            newTransactions.Select(x => transactionEntity.ToNewSplittedEntity(x));

        public static IEnumerable<TransactionModel> ToDto(this IEnumerable<TransactionEntity> entities)
            => entities.Select(x => x.ToDto());

        public static TransactionModel ToDto(this TransactionEntity entity) =>
            new()
            {
                Id = entity.Id,
                BankAccountId = entity.BankAccountId,
                Name = entity.Name,
                Date = entity.Date,
                Amount = entity.Amount,
                CategoryId = entity.CategoryId,
                Description = entity.Description,
                SourceAccountNumber = entity.AccountNumber,
                DestinationAccountNumber = entity.DestinationAccount
            };

        public static TransactionEntity ToNewSplittedEntity(
            this TransactionEntity transactionEntity,
            UpsertTransactionModel model) =>
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

        public static void UpdateFromUpsertModel(this TransactionEntity entity, UpsertTransactionModel upsertTransactionModel)
        {
            entity.Id = upsertTransactionModel.Id;
            entity.Description = upsertTransactionModel.Description;
            entity.Name = upsertTransactionModel.Name;
            entity.Amount = upsertTransactionModel.Amount;
            entity.Date = upsertTransactionModel.Date;
            entity.AccountNumber = upsertTransactionModel.SourceAccountNumber;
            entity.DestinationAccount = upsertTransactionModel.DestinationAccountNumber;
            entity.CategoryId = upsertTransactionModel.CategoryId;
        }
    }
}
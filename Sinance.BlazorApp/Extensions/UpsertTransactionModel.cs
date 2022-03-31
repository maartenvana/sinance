using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Storage.Entities;

namespace Sinance.BlazorApp.Extensions
{
    public static class UpsertTransactionModelExtensions
    {
        public static TransactionEntity ToNewTransactionEntity(this UpsertTransactionModel model, int userId) => new TransactionEntity
        {
            Id = model.Id,
            AccountNumber = model.SourceAccountNumber,
            DestinationAccount = model.DestinationAccountNumber,
            Amount = model.Amount,
            BankAccountId = model.BankAccountId.Value,
            CategoryId = model.CategoryId,
            Description = model.Description,
            Date = model.Date,
            Name = model.Name,
            UserId = userId
        };
    }
}

using Sinance.BlazorApp.Business.Model.Transaction;

namespace Sinance.BlazorApp.Extensions;

public static class TransactionModelExtensions
{
    public static void MarkAsNew(this UpsertTransactionModel transactionModel)
    {
        transactionModel.Id = 0;
    }

    public static UpsertTransactionModel ToUpsertModel(this TransactionModel transactionModel, int bankAccountId) =>
        new()
        {
            Amount = transactionModel.Amount,
            CategoryId = transactionModel.CategoryId,
            Date = transactionModel.Date,
            Description = transactionModel.Description,
            DestinationAccountNumber = transactionModel.DestinationAccountNumber,
            Id = transactionModel.Id,
            BankAccountId = bankAccountId,
            Name = transactionModel.Name,
            SourceAccountNumber = transactionModel.SourceAccountNumber
        };
}

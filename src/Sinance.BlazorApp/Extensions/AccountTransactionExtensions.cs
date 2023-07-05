using Sinance.BlazorApp.Business.Model.Transaction;
using Sinance.Domain.Model;

namespace Sinance.BlazorApp.Extensions;

public static class AccountTransactionExtensions
{
    public static TransactionModel ToTransactionModel(this AccountTransaction accountTransaction)
    {
        return new TransactionModel
        {
            Amount = accountTransaction.Amount,
            BankAccountId = accountTransaction.BankAccountId,
            CategoryId = accountTransaction.CategoryId,
            Date = accountTransaction.Date,
            Description = accountTransaction.Description,
            DestinationAccountNumber = accountTransaction.DestinationAccount,
            Id = accountTransaction.Id,
            Name = accountTransaction.Name,
            SourceAccountNumber = accountTransaction.AccountNumber,
        };
    }
}

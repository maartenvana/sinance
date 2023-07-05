using Sinance.Communication.Model.Transaction;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.Business.Extensions;

public static class TransactionExtensions
{
    public static List<TransactionModel> ToDto(this List<TransactionEntity> transactions) => transactions.Select(x => x.ToDto()).ToList();

    public static TransactionModel ToDto(this TransactionEntity transaction) =>
        new()
        {
            Id = transaction.Id,
            Name = transaction.Name,
            BankAccountId = transaction.BankAccountId,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Category = transaction.Category?.ToTransactionCategoryDto(),
            Description = transaction.Description,
            DestinationAccount = transaction.DestinationAccount
        };

    public static TransactionEntity ToNewEntity(this TransactionModel transactionModel, int userId) =>
        new()
        {
            Name = transactionModel.Name,
            Description = transactionModel.Description,
            DestinationAccount = transactionModel.DestinationAccount,
            Amount = transactionModel.Amount,
            Date = transactionModel.Date,
            BankAccountId = transactionModel.BankAccountId,
            AccountNumber = transactionModel.FromAccount,
            UserId = userId,
            CategoryId = transactionModel.Category?.CategoryId
        };

    public static ImportTransactionEntity ToNewImportEntity(this TransactionModel transactionModel, int userId) =>
        new()
        {
            Name = transactionModel.Name,
            Description = transactionModel.Description,
            DestinationAccount = transactionModel.DestinationAccount,
            Amount = transactionModel.Amount,
            Date = transactionModel.Date,
            BankAccountId = transactionModel.BankAccountId,
            AccountNumber = transactionModel.FromAccount,
            UserId = userId
        };

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
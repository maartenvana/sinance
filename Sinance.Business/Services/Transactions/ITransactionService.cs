using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Transactions
{
    public interface ITransactionService
    {
        Task<TransactionModel> ClearTransactionCategoriesForCurrentUser(int transactionId);

        Task<TransactionModel> CreateTransactionForCurrentUser(TransactionModel transactionModel);

        Task DeleteTransactionForCurrentUser(int transactionId);

        Task<TransactionModel> GetTransactionByIdForCurrentUser(int transactionId);

        Task<IEnumerable<TransactionModel>> GetTransactionsForBankAccountForCurrentUser(int bankAccountId, int count, int skip);

        Task<IEnumerable<TransactionModel>> GetTransactionsForMonthForCurrentUser(int year, int month);

        Task<TransactionModel> OverwriteTransactionCategoriesForCurrentUser(int transactionId, int categoryId);

        Task<TransactionModel> UpdateTransactionForCurrentUser(TransactionModel transactionModel);
    }
}
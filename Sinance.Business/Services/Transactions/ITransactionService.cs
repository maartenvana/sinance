using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Transactions
{
    public interface ITransactionService
    {
        Task<TransactionModel> ClearTransactionCategories(int userId, int transactionId);

        Task<TransactionModel> CreateTransaction(int userId, TransactionModel transactionModel);

        Task DeleteTransactionForUser(int userId, int transactionId);

        Task<TransactionModel> GetTransactionByIdForUserId(int userId, int transactionId);

        Task<IEnumerable<TransactionModel>> GetTransactionsForBankAccount(int currentUserId, int bankAccountId, int count, int skip);

        Task<IEnumerable<TransactionModel>> GetTransactionsForUserForMonth(int userId, int year, int month);

        Task<TransactionModel> OverwriteTransactionCategories(int userId, int transactionId, int categoryId);

        Task<TransactionModel> UpdateTransaction(int userId, TransactionModel transactionModel);
    }
}
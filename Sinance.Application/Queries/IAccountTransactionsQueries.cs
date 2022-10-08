using Sinance.Contracts.Transaction;
using Sinance.Domain.Model;

namespace Sinance.Application.Queries
{
    public interface IAccountTransactionQueries
    {
        Task<List<AccountTransaction>> FindTransactionsAsync(FindAccountTransactionsFilter filter);
    }
}
using Sinance.BlazorApp.Business.Model.Transaction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Services;

public interface ITransactionService
{
    Task<List<TransactionModel>> SearchTransactionsPaged(SearchTransactionsFilterModel filter);
}
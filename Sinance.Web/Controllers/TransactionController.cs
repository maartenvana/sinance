using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Sinance.Business.Services.Transactions;

namespace Sinance.Controllers
{
    /// <summary>
    /// Transaction controller for various global operations on transactions
    /// </summary>
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Loads more transactions for the edit table
        /// </summary>
        /// <param name="bankAccountId">Bank account to load transactions of</param>
        /// <param name="skipTransactions">Ammount of transactions to skip</param>
        /// <param name="takeTransactions">Ammount of transactions to load</param>
        /// <returns>Partial view containing rows of transactions</returns>
        [HttpPost]
        public async Task<IActionResult> LoadMoreEditTransactionsPartial(int bankAccountId, int skipTransactions, int takeTransactions)
        {
            var transactions = await _transactionService.GetTransactionsForBankAccountForCurrentUser(bankAccountId, takeTransactions, skip: skipTransactions);

            return PartialView(transactions);
        }
    }
}
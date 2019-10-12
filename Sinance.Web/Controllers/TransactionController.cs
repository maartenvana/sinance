using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using Sinance.Storage;
using System.Threading.Tasks;
using Sinance.Business.Services;

namespace Sinance.Controllers
{
    /// <summary>
    /// Transaction controller for various global operations on transactions
    /// </summary>
    public class TransactionController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public TransactionController(
            IBankAccountService bankAccountService,
            Func<IUnitOfWork> unitOfWork)
        {
            _bankAccountService = bankAccountService;
            _unitOfWork = unitOfWork;
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
            ActionResult result;

            using var unitOfWork = _unitOfWork();

            var bankAccounts = await _bankAccountService.GetAllBankAccountsForCurrentUser();

            if (!bankAccounts.Any(x => x.Id == bankAccountId))
            {
                return NotFound();
            }

            var allTransactions = await unitOfWork.TransactionRepository.FindAll(item => item.BankAccountId == bankAccountId);

            var transactions = allTransactions.OrderByDescending(item => item.Date).Skip(skipTransactions).Take(takeTransactions).ToList();
            result = PartialView(transactions);

            return result;
        }
    }
}
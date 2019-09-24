using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using Sinance.Storage;

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
            Func<IUnitOfWork> unitOfWork,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Loads more transactions for the edit table
        /// </summary>
        /// <param name="bankAccountId">Bank account to load transactions of</param>
        /// <param name="skipTransactions">Ammount of transactions to skip</param>
        /// <param name="takeTransactions">Ammount of transactions to load</param>
        /// <returns>Partial view containing rows of transactions</returns>
        [HttpPost]
        public IActionResult LoadMoreEditTransactionsPartial(int bankAccountId, int skipTransactions, int takeTransactions)
        {
            ActionResult result;

            using (var unitOfWork = _unitOfWork())
            {
                IList<Transaction> allTransactions = unitOfWork.TransactionRepository.FindAll(item => item.BankAccountId == bankAccountId);

                List<Transaction> transactions = allTransactions.OrderByDescending(item => item.Date).Skip(skipTransactions).Take(takeTransactions).ToList();
                result = PartialView(transactions);

                return result;
            }
        }
    }
}
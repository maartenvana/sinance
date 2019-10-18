using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sinance.Business.Classes;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.Transactions;
using Sinance.Communication.Model.Transaction;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Controllers
{
    /// <summary>
    /// Overview controller
    /// </summary>
    [Authorize]
    public class AccountOverviewController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthenticationService _sessionService;
        private readonly ITransactionService _transactionService;

        public AccountOverviewController(
            IAuthenticationService sessionService,
            ICategoryService categoryService,
            IBankAccountService bankAccountService,
            ITransactionService transactionService)
        {
            _sessionService = sessionService;
            _categoryService = categoryService;
            _bankAccountService = bankAccountService;
            _transactionService = transactionService;
        }

        /// <summary>
        /// Starts the add action for a model
        /// </summary>
        /// <param name="bankAccountId">Id of the bank account to add the model to</param>
        /// <returns>Partial view for creating the model</returns>
        public IActionResult AddTransaction(int bankAccountId)
        {
            return PartialView("UpsertTransactionPartial", new TransactionModel
            {
                BankAccountId = bankAccountId,
                Date = DateTime.Now
            });
        }

        /// <summary>
        /// Deletes a model
        /// </summary>
        /// <param name="transactionId">Id of model to delete</param>
        /// <returns>Result of the delete action</returns>
        public async Task<IActionResult> DeleteTransaction(int transactionId, int bankAccountId)
        {
            ActionResult result;

            var currentUserId = await _sessionService.GetCurrentUserId();

            try
            {
                await _transactionService.DeleteTransactionForUser(currentUserId, transactionId);

                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.TransactionDeleted);
                result = RedirectToAction("Index", new { bankAccountId });
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.TransactionNotFound);
                result = RedirectToAction("Index", "Home");
            }

            return result;
        }

        /// <summary>
        /// Starts the edit action for a model
        /// </summary>
        /// <param name="transactionId">Id of the model to edit</param>
        /// <returns>Partial view for editing the model</returns>
        public async Task<IActionResult> EditTransaction(int transactionId)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            try
            {
                var transaction = await _transactionService.GetTransactionByIdForUserId(currentUserId, transactionId);
                var userCategories = await _categoryService.GetAllCategoriesForUser(currentUserId);

                var availableCategories = new List<SelectListItem>{
                    new SelectListItem {
                        Text =  Resources.NoCategory,
                        Value = "0"
                    }
                };
                availableCategories.AddRange(userCategories.Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(CultureInfo.InvariantCulture)
                }));

                var model = new EditTransactionModel
                {
                    AvailableCategories = availableCategories,
                    Transaction = transaction
                };

                return PartialView("UpsertTransactionPartial", model);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.TransactionNotFound);
            }

            return View("Index");
        }

        /// <summary>
        /// Index action for account overview
        /// </summary>
        /// <param name="bankAccountId">Id of the bank account to show</param>
        /// <returns>View containing the overview</returns>
        public async Task<IActionResult> Index(int bankAccountId)
        {
            try
            {
                var currentUserId = await _sessionService.GetCurrentUserId();
                var bankAccount = await _bankAccountService.GetBankAccountByIdForUser(currentUserId, bankAccountId);
                var transactions = await _transactionService.GetTransactionsForBankAccount(currentUserId, bankAccountId, 200, skip: 0);
                var availableCategories = await _categoryService.GetAllCategoriesForUser(currentUserId);

                var model = new AccountOverviewModel
                {
                    Account = bankAccount,
                    Transactions = transactions.Take(200).ToList(),
                    AccountBalance = bankAccount.CurrentBalance.GetValueOrDefault(),
                    AvailableCategories = availableCategories
                };

                return View("index", model);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                return View("index");
            }
        }

        /// <summary>
        /// Changes the category for the given model
        /// </summary>
        /// <param name="transactionId">Id of the model to the change the category of</param>
        /// <param name="categoryId">Id of category to set</param>
        /// <returns>Result of update</returns>
        [HttpPost]
        public async Task<IActionResult> QuickChangeTransactionCategory(int transactionId, int categoryId)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            try
            {
                var transaction = await _transactionService.OverwriteTransactionCategories(currentUserId, transactionId, categoryId);

                return PartialView("TransactionEditRow", transaction);
            }
            catch (NotFoundException)
            {
                return Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.CouldNotUpdateTransactionCategory
                });
            }
        }

        /// <summary>
        /// Removes the category for the given transaction
        /// </summary>
        /// <param name="transactionId">Id of the transaction to the change the category of</param>
        /// <returns>Result of update</returns>
        [HttpPost]
        public async Task<IActionResult> QuickRemoveTransactionCategory(int transactionId)
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            try
            {
                var transaction = await _transactionService.ClearTransactionCategories(currentUserId, transactionId);

                return PartialView("TransactionEditRow", transaction);
            }
            catch (NotFoundException)
            {
                return Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.CouldNotUpdateTransactionCategory
                });
            }
        }

        /// <summary>
        /// Upserts a model to the database
        /// </summary>
        /// <param name="model">Transaction to upsert</param>
        /// <returns>Result of the model</returns>
        [HttpPost]
        public async Task<IActionResult> UpsertTransaction(TransactionModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = await _sessionService.GetCurrentUserId();

                    if (model.Id > 0)
                    {
                        await _transactionService.UpdateTransaction(currentUserId, model);

                        return Json(new SinanceJsonResult
                        {
                            Success = true
                        });
                    }
                    else
                    {
                        await _transactionService.CreateTransaction(currentUserId, model);

                        return Json(new SinanceJsonResult
                        {
                            Success = true
                        });
                    }
                }
                catch (NotFoundException)
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                }
            }

            return PartialView("UpsertTransactionPartial", model);
        }
    }
}
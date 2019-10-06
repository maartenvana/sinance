using Sinance.Business.Classes;
using Sinance.Business.Handlers;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Overview controller
    /// </summary>
    [Authorize]
    public class AccountOverviewController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public AccountOverviewController(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService sessionService,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Starts the add action for a model
        /// </summary>
        /// <param name="bankAccountId">Id of the bank account to add the model to</param>
        /// <returns>Partial view for creating the model</returns>
        public async Task<IActionResult> AddTransaction(int bankAccountId)
        {
            ActionResult result;
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
            var bankAccount = bankAccounts.SingleOrDefault(item => item.Id == bankAccountId);

            if (bankAccount != null)
            {
                result = PartialView("UpsertTransactionPartial", new TransactionModel
                {
                    BankAccountId = bankAccountId,
                    Date = DateTime.Now
                });
            }
            else
            {
                result = PartialView("UpsertTransactionPartial");
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
            }

            return result;
        }

        /// <summary>
        /// Deletes a model
        /// </summary>
        /// <param name="transactionId">Id of model to delete</param>
        /// <returns>Result of the delete action</returns>
        public async Task<IActionResult> DeleteTransaction(int transactionId)
        {
            ActionResult result;

            var currentUserId = await _sessionService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId &&
                                                                                item.UserId == currentUserId);

            if (transaction != null)
            {
                if (transaction.TransactionCategories != null)
                {
                    unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);
                }

                unitOfWork.TransactionRepository.Delete(transaction);
                await unitOfWork.SaveAsync();

                await TransactionHandler.UpdateCurrentBalance(unitOfWork, transaction.BankAccountId, currentUserId);

                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.TransactionDeleted);
                result = RedirectToAction("Index", new { @bankAccountId = transaction.BankAccountId });
            }
            else
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

            using var unitOfWork = _unitOfWork();
            var transaction = await unitOfWork.TransactionRepository.FindSingle(item => item.Id == transactionId &&
                           item.UserId == currentUserId);
            TransactionModel transactionModel = null;

            if (transaction == null)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.TransactionNotFound);
            }
            else
            {
                var allCategories = await unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId);

                var availableCategories = new List<SelectListItem>{
                    new SelectListItem {
                        Text =  Resources.NoCategory,
                        Value = "0"
                    }
                };
                availableCategories.AddRange(allCategories.ConvertAll(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(CultureInfo.InvariantCulture)
                }));

                transactionModel = TransactionModel.CreateTransactionModel(transaction);
                transactionModel.AvailableCategories = availableCategories;
            }

            return PartialView("UpsertTransactionPartial", transactionModel);
        }

        /// <summary>
        /// Index action for account overview
        /// </summary>
        /// <param name="bankAccountId">Id of the bank account to show</param>
        /// <returns>View containing the overview</returns>
        public async Task<IActionResult> Index(int bankAccountId)
        {
            ActionResult result;
            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
            var currentUserId = await _sessionService.GetCurrentUserId();

            var bankAccount = bankAccounts.SingleOrDefault(item => item.Id == bankAccountId);

            if (bankAccount != null)
            {
                using var unitOfWork = _unitOfWork();
                var transactions = await unitOfWork.TransactionRepository
                    .FindAll(item => item.BankAccount.Id == bankAccountId,
                                        includeProperties: new string[] {
                                                nameof(Transaction.TransactionCategories),
                                                $"{nameof(Transaction.TransactionCategories)}.{nameof(TransactionCategory.Category)}"
                                        });
                transactions = transactions.OrderByDescending(item => item.Date).ToList();

                var categories = await unitOfWork.CategoryRepository.FindAll(item => item.UserId == currentUserId);

                var model = new AccountOverviewModel
                {
                    Account = bankAccount,
                    Transactions = transactions.Take(200).ToList(),
                    AccountBalance = transactions.Sum(item => item.Amount) + bankAccount.StartBalance,
                    AvailableCategories = categories
                };

                result = View("index", model);
            }
            else
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                result = View("index");
            }

            return result;
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

            using var unitOfWork = _unitOfWork();
            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId &&
                           item.UserId == currentUserId,
                           includeProperties: "TransactionCategories");
            var category = await unitOfWork.CategoryRepository.FindSingle(item => item.Id == categoryId &&
                                                                        item.UserId == currentUserId);

            ActionResult result;
            if (transaction != null && category != null)
            {
                // Remove any previous assigned categories
                unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);

                // Insert the new link
                unitOfWork.TransactionCategoryRepository.Insert(new TransactionCategory
                {
                    TransactionId = transaction.Id,
                    CategoryId = category.Id
                });

                await unitOfWork.SaveAsync();

                result = PartialView("TransactionEditRow", transaction);
            }
            else
            {
                result = Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.CouldNotUpdateTransactionCategory
                });
            }

            return result;
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

            using var unitOfWork = _unitOfWork();

            var transaction = await unitOfWork.TransactionRepository.FindSingleTracked(item => item.Id == transactionId &&
                                   item.UserId == currentUserId,
                                   includeProperties: nameof(Transaction.TransactionCategories));
            ActionResult result;
            if (transaction != null)
            {
                // Remove any previous assigned categories
                unitOfWork.TransactionCategoryRepository.DeleteRange(transaction.TransactionCategories);
                await unitOfWork.SaveAsync();

                result = PartialView("TransactionEditRow", transaction);
            }
            else
            {
                result = Json(new SinanceJsonResult
                {
                    Success = false,
                    ErrorMessage = Resources.CouldNotUpdateTransactionCategory
                });
            }

            return result;
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
                throw new ArgumentNullException(nameof(model));

            ActionResult result;
            if (ModelState.IsValid)
            {
                var currentUserId = await _sessionService.GetCurrentUserId();
                var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

                // Check if we have access to the bank account
                var bankAccount = bankAccounts.SingleOrDefault(item => item.Id == model.BankAccountId);

                using var unitOfWork = _unitOfWork();
                if (bankAccount != null)
                {
                    if (model.Id > 0)
                    {
                        var updateTransaction = await unitOfWork.TransactionRepository.FindSingleTracked(item =>
                            item.Id == model.Id && item.UserId == currentUserId);

                        if (updateTransaction != null)
                        {
                            updateTransaction.Update(name: model.Name,
                                description: model.Description,
                                destinationAccount: model.DestinationAccount,
                                amount: model.Amount,
                                date: model.Date,
                                bankAccountId: model.BankAccountId);

                            unitOfWork.TransactionRepository.Update(updateTransaction);
                            await unitOfWork.SaveAsync();

                            await TransactionHandler.UpdateCurrentBalance(unitOfWork, bankAccount.Id, currentUserId);

                            result = Json(new SinanceJsonResult
                            {
                                Success = true
                            });
                        }
                        else
                        {
                            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.TransactionNotFound);
                            result = PartialView("UpsertTransactionPartial", model);
                        }
                    }
                    else
                    {
                        var insertTransaction = new Transaction();
                        insertTransaction.Update(name: model.Name,
                                description: model.Description,
                                destinationAccount: model.DestinationAccount,
                                amount: model.Amount,
                                date: model.Date,
                                bankAccountId: model.BankAccountId);
                        insertTransaction.UserId = currentUserId;

                        unitOfWork.TransactionRepository.Insert(insertTransaction);
                        await unitOfWork.SaveAsync();

                        await TransactionHandler.UpdateCurrentBalance(unitOfWork, bankAccount.Id, currentUserId);

                        result = Json(new SinanceJsonResult
                        {
                            Success = true
                        });
                    }
                }
                else
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                    result = PartialView("UpsertTransactionPartial", model);
                }
            }
            else
            {
                result = PartialView("UpsertTransactionPartial", model);
            }

            return result;
        }
    }
}
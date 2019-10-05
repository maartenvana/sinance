using Sinance.Business.Handlers;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Bank account controller
    /// </summary>
    [Authorize]
    public class BankAccountController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BankAccountController(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService sessionService,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Starts the action of adding an account
        /// </summary>
        /// <returns>ActionResult with the add action page</returns>
        public IActionResult AddAccount()
        {
            BankAccountModel bankAccount = new BankAccountModel();

            return View("UpsertAccount", bankAccount);
        }

        /// <summary>
        /// Start an edit account action
        /// </summary>
        /// <param name="accountId">Id of account to edit</param>
        /// <returns>Actionresult with details of the account</returns>
        public async Task<IActionResult> EditAccount(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentOutOfRangeException(nameof(accountId));

            ActionResult result;

            var bankAccounts = await this._bankAccountService.GetAllBankAccountsForCurrentUser();

            BankAccount bankAccount = bankAccounts.SingleOrDefault(item => item.Id == accountId);

            if (bankAccount == null)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                result = RedirectToAction("Index");
            }
            else
            {
                BankAccountModel model = BankAccountModel.CreateBankAccountModel(bankAccount);
                result = View("UpsertAccount", model);
            }

            return result;
        }

        /// <summary>
        /// Index page of the bankaccount controller
        /// </summary>
        /// <returns>Default view</returns>
        public async Task<IActionResult> Index()
        {
            var bankAccounts = await this._bankAccountService.GetAllBankAccountsForCurrentUser();

            return View(bankAccounts);
        }

        /// <summary>
        /// Removes the given account
        /// </summary>
        /// <param name="accountId">Id of account to remove</param>
        /// <returns>ActionResult with details of the remove action</returns>
        public async Task<IActionResult> RemoveAccount(int accountId)
        {
            if (accountId <= 0)
                throw new ArgumentOutOfRangeException(nameof(accountId));

            var currentUserId = await this._sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                BankAccount bankAccount = unitOfWork.BankAccountRepository.FindSingleTracked(item => item.Id == accountId && item.UserId == currentUserId,
                includeProperties: "Transactions");

                if (bankAccount != null)
                {
                    unitOfWork.TransactionRepository.DeleteRange(bankAccount.Transactions);
                    unitOfWork.BankAccountRepository.Delete(bankAccount.Id);

                    await unitOfWork.SaveAsync();
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success,
                        ViewBag.Message = Resources.BankAccountRemoved);
                }
                else
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error,
                        ViewBag.Message = Resources.BankAccountNotFound);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Upserts an account
        /// </summary>
        /// <param name="model">Model to use for upsert</param>
        /// <returns>ActionResult with the outcome</returns>
        public async Task<IActionResult> UpsertAccount(BankAccountModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            ActionResult result = View(model);

            if (ModelState.IsValid)
            {
                var currentUserId = await this._sessionService.GetCurrentUserId();

                using (var unitOfWork = _unitOfWork())
                {
                    // If the account already exists update the current one
                    if (model.Id > 0)
                    {
                        var bankAccounts = await this._bankAccountService.GetAllBankAccountsForCurrentUser();

                        BankAccount bankAccount = bankAccounts.SingleOrDefault(item => item.Id == model.Id);
                        if (bankAccount != null)
                        {
                            bankAccount.Update(model.Name, model.StartBalance, model.Disabled, model.AccountType, model.IncludeInProfitLossGraph);

                            unitOfWork.BankAccountRepository.Update(bankAccount);
                            await unitOfWork.SaveAsync();
                            await TransactionHandler.UpdateCurrentBalance(unitOfWork, bankAccount.Id, currentUserId);
                            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.BankAccountUpdated);
                        }
                        else
                            ModelState.AddModelError("Message", Resources.BankAccountNotFound);
                    }
                    else
                    {
                        if (!unitOfWork.BankAccountRepository.FindAllTracked(item => item.Name == model.Name).Any())
                        {
                            BankAccount insertBankAccount = new BankAccount();
                            insertBankAccount.Update(model.Name, model.StartBalance, model.Disabled, model.AccountType, model.IncludeInProfitLossGraph);
                            insertBankAccount.CurrentBalance = model.StartBalance;
                            insertBankAccount.UserId = currentUserId;

                            unitOfWork.BankAccountRepository.Insert(insertBankAccount);
                            await unitOfWork.SaveAsync();

                            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.BankAccountCreated);
                        }
                        else
                            ModelState.AddModelError("Message", Resources.BankAccountAlreadyExists);
                    }
                }

                // If the model state is still valid, update the session and redirect to the index page
                if (ModelState.IsValid)
                {
                    result = RedirectToAction("Index");
                }
            }

            return result;
        }
    }
}
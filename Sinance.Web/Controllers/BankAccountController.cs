using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.BankAccount;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System;
using System.Threading.Tasks;

namespace Sinance.Controllers
{
    /// <summary>
    /// Bank account controller
    /// </summary>
    [Authorize]
    public class BankAccountController : Controller
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Starts the action of adding an account
        /// </summary>
        /// <returns>ActionResult with the add action page</returns>
        public IActionResult AddAccount()
        {
            var bankAccount = new BankAccountModel();

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
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }

            try
            {
                var bankAccount = await _bankAccountService.GetBankAccountByIdForCurrentUser(accountId);
                return View("UpsertAccount", bankAccount);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Index page of the bankaccount controller
        /// </summary>
        /// <returns>Default view</returns>
        public async Task<IActionResult> Index()
        {
            var bankAccounts = await _bankAccountService.GetAllBankAccountsForCurrentUser();

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
            {
                throw new ArgumentOutOfRangeException(nameof(accountId));
            }

            try
            {
                await _bankAccountService.DeleteBankAccountByIdForCurrentUser(accountId);

                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success,
                    ViewBag.Message = Resources.BankAccountRemoved);
            }
            catch (NotFoundException)
            {
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
            {
                throw new ArgumentNullException(nameof(model));
            }

            ActionResult result = View(model);

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Id > 0)
                    {
                        await _bankAccountService.UpdateBankAccountForCurrentUser(model);
                        TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.BankAccountCreated);
                    }
                    else
                    {
                        await _bankAccountService.CreateBankAccountForCurrentUser(model);
                        TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.BankAccountUpdated);
                    }
                }
                catch (NotFoundException)
                {
                    ModelState.AddModelError("Message", Resources.BankAccountNotFound);
                }
                catch (AlreadyExistsException)
                {
                    ModelState.AddModelError("Message", Resources.BankAccountNotFound);
                }
                catch (ArgumentException exc)
                {
                    ModelState.AddModelError("Message", exc.Message);
                }

                return RedirectToAction("Index");
            }

            return result;
        }
    }
}
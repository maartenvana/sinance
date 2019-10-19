using Sinance.Business.Handlers;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.BankAccount;
using Sinance.Business.Exceptions;

namespace Sinance.Controllers
{
    /// <summary>
    /// Bank account controller
    /// </summary>
    [Authorize]
    public class BankAccountController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(
            IAuthenticationService authenticationService,
            IBankAccountService bankAccountService)
        {
            _authenticationService = authenticationService;
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
                var currentUserId = await _authenticationService.GetCurrentUserId();
                var bankAccount = await _bankAccountService.GetBankAccountByIdForUser(currentUserId, accountId);
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
            var currentUserId = await _authenticationService.GetCurrentUserId();
            var bankAccounts = await _bankAccountService.GetAllBankAccountsForUser(currentUserId);

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
                var currentUserId = await _authenticationService.GetCurrentUserId();
                await _bankAccountService.DeleteBankAccountByIdForUser(currentUserId, accountId);

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
                var currentUserId = await _authenticationService.GetCurrentUserId();

                try
                {
                    if (model.Id > 0)
                    {
                        await _bankAccountService.UpdateBankAccount(currentUserId, model);
                        TempDataHelper.SetTemporaryMessage(TempData, MessageState.Success, Resources.BankAccountCreated);
                    }
                    else
                    {
                        await _bankAccountService.CreateBankAccountForCurrentUser(currentUserId, model);
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
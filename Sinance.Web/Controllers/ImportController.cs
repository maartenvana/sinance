using Sinance.Business.Handlers;
using Sinance.Business.Services;
using Sinance.Domain.Entities;
using Sinance.Web.Model;
using Sinance.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Web.Helper;
using Sinance.Storage;
using Sinance.Business.Services.Authentication;

namespace Sinance.Controllers
{
    /// <summary>
    /// Controller handling the importing of csv files
    /// </summary>
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly SelectListHelper _selectListHelper;
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ImportController(
            Func<IUnitOfWork> unitOfWork,
            SelectListHelper selectListHelper,
            IAuthenticationService sessionService,
            IBankAccountService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _selectListHelper = selectListHelper;
            _sessionService = sessionService;
            _bankAccountService = bankAccountService;
        }

        /// <summary>
        /// Prepares a file upload for processing
        /// </summary>
        /// <param name="file">File to prepare for import</param>
        /// <param name="model">Posted model</param>
        /// <returns>Preparation result</returns>
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, ImportModel model)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Clear the previous cache (if it exists)
            FinanceCacheHandler.ClearCache(await ConstructImportModelCacheKey());

            using var unitOfWork = _unitOfWork();

            var importBank = await unitOfWork.ImportBankRepository.FindSingle(item => item.Id == model.ImportBankId);

            if (importBank != null)
            {
                var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

                var importMappings = await unitOfWork.ImportMappingRepository.FindAll(item => item.ImportBankId == importBank.Id);
                var bankAccount = bankAccounts.SingleOrDefault(item => item.Id == model.BankAccountId);

                if (bankAccount != null)
                {
                    try
                    {
                        using var stream = file.OpenReadStream();

                        model.ImportRows = await ImportHandler.ProcessImport(
                            unitOfWork,
                            fileInputStream: stream,
                            userId: bankAccount.UserId,
                            importMappings: importMappings,
                            importBank: importBank,
                            bankAccountId: bankAccount.Id);

                        // Place the import model in the cache for easy acces later
                        FinanceCacheHandler.Cache(key: await ConstructImportModelCacheKey(),
                            contentAction: () => model,
                            slidingExpiration: false,
                            expirationTimeSpan: new TimeSpan(0, 0, 15, 0));

                        return View("ImportResult", model);
                    }
                    catch (Exception)
                    {
                        TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, message: Resources.ErrorWhileProcessingImport);
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, message: Resources.BankAccountNotFound);
                    return RedirectToAction("Index");
                }
            }

            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, message: Resources.Error);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Index page for importing
        /// </summary>
        /// <returns>ActionResult for user</returns>
        public async Task<IActionResult> Index()
        {
            using var unitOfWork = _unitOfWork();

            // Retrieve all import banks
            var importBanks = await unitOfWork.ImportBankRepository.FindAll(item => item.Id > 0);

            return View("Index", new ImportModel
            {
                AvailableImportBanks = importBanks
            });
        }

        /// <summary>
        /// Save action for import
        /// </summary>
        /// <param name="model">Model to save</param>
        /// <returns>View with the save result</returns>
        [HttpPost]
        public async Task<IActionResult> SaveImport(ImportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var cachedModel = FinanceCacheHandler.RetrieveCache<ImportModel>(await ConstructImportModelCacheKey());
            if (cachedModel != null)
            {
                var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();

                // Get the bank account from session to prevent malicious adding of transactions
                var bankAccount = bankAccounts.SingleOrDefault(item => item.Id == model.BankAccountId);

                if (bankAccount != null)
                {
                    using var unitOfWork = _unitOfWork();

                    var skippedTransactions = model.ImportRows.Count(item => item.ExistsInDatabase || !item.Import);
                    var savedTransactions = await ImportHandler.SaveImportResultToDatabase(unitOfWork,
                        bankAccountId: bankAccount.Id,
                        importRows: model.ImportRows,
                        cachedImportRows: cachedModel.ImportRows,
                        userId: bankAccount.UserId);

                    // Update the current balance of bank account and refresh them
                    await TransactionHandler.UpdateCurrentBalance(unitOfWork,
                        bankAccountId: bankAccount.Id,
                        userId: bankAccount.UserId);

                    // Clear the cache entry
                    FinanceCacheHandler.ClearCache(await ConstructImportModelCacheKey());

                    var message = string.Format(CultureInfo.CurrentCulture, Resources.TransactionsAddedAndSkippedFormat, savedTransactions, skippedTransactions);

                    TempDataHelper.SetTemporaryMessage(tempData: TempData,
                        state: savedTransactions != 0 ? MessageState.Success : MessageState.Warning,
                        message: message);
                    return RedirectToAction("Index", "AccountOverview", new { @bankAccountId = model.BankAccountId });
                }
                else
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                    return RedirectToAction("Index", "Home");
                }
            }

            TempDataHelper.SetTemporaryMessage(TempData, MessageState.Warning, Resources.ImportTimeOut);
            return View("Index");
        }

        /// <summary>
        /// Starts the import for the given bank account type
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> StartImport(ImportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableAccounts = await _selectListHelper.CreateActiveBankAccountSelectList(BankAccountType.Checking);

            return View("StartImport", model);
        }

        /// <summary>
        /// Cache key for the import model
        /// </summary>
        private async Task<string> ConstructImportModelCacheKey()
        {
            var currentUserId = await _sessionService.GetCurrentUserId();

            return string.Format(CultureInfo.CurrentCulture, "ImportRows_{0}", currentUserId);
        }
    }
}
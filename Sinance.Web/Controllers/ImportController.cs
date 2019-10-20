using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Imports;
using Sinance.Communication.Model.Import;
using Sinance.Web;
using Sinance.Web.Helper;
using Sinance.Web.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Controllers
{
    /// <summary>
    /// Controller handling the importing of csv files
    /// </summary>
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IImportService _importService;

        public ImportController(
            IImportService importService,
            IBankAccountService bankAccountService)
        {
            _importService = importService;
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
            {
                throw new ArgumentNullException(nameof(file));
            }

            try
            {
                using var stream = file.OpenReadStream();

                await _importService.CreateImportPreview(stream, model);

                return View("ImportResult", model);
            }
            catch (NotFoundException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, message: Resources.BankAccountNotFound);
                return RedirectToAction("Index");
            }
            catch (ImportFileException)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, message: Resources.ErrorWhileProcessingImport);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, message: Resources.Error);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Index page for importing
        /// </summary>
        /// <returns>ActionResult for user</returns>
        public async Task<IActionResult> Index()
        {
            var importBanks = await _importService.GetImportBanks();

            return View("Index", new ImportModel
            {
                AvailableImportBanks = importBanks.ToList()
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
            try
            {
                var (skippedTransactions, savedTransactions) = await _importService.SaveImport(model);

                var message = string.Format(CultureInfo.CurrentCulture, Resources.TransactionsAddedAndSkippedFormat, savedTransactions, skippedTransactions);

                TempDataHelper.SetTemporaryMessage(tempData: TempData,
                    state: savedTransactions != 0 ? MessageState.Success : MessageState.Warning,
                    message: message);
                return RedirectToAction("Index", "AccountOverview", new { @bankAccountId = model.BankAccountId });
            }
            catch (NotFoundException exc)
            {
                if (exc.ItemName == nameof(ImportModel))
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.ImportTimeOut);
                }
                else
                {
                    TempDataHelper.SetTemporaryMessage(TempData, MessageState.Error, Resources.BankAccountNotFound);
                }
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Starts the import for the given bank account type
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> StartImport(ImportModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
            model.AvailableAccounts = bankAccounts;

            return View("StartImport", model);
        }
    }
}
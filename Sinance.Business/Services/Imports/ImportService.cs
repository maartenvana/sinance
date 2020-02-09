using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Handlers;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.Import;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Imports
{
    public class ImportService : IImportService
    {
        private readonly IBankAccountCalculationService _bankAccountCalculationService;
        private readonly Func<IUnitOfWork> _unitOfWork;
        private readonly IUserIdProvider _userIdProvider;

        public ImportService(
            Func<IUnitOfWork> unitOfWork,
            IUserIdProvider userIdProvider,
            IBankAccountCalculationService bankAccountService)
        {
            _unitOfWork = unitOfWork;
            _userIdProvider = userIdProvider;
            _bankAccountCalculationService = bankAccountService;
        }

        public async Task<ImportModel> CreateImportPreview(Stream fileStream, ImportModel model)
        {
            var importCacheKey = CreateImportCacheKey(_userIdProvider.GetCurrentUserId());
            SinanceCacheHandler.ClearCache(importCacheKey);

            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingle(x => x.Id == model.BankAccountId);
            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            var importBank = await unitOfWork.ImportBankRepository.FindSingle(item => item.Id == model.ImportBankId);

            if (importBank == null)
            {
                throw new NotFoundException(nameof(ImportBankEntity));
            }
            var importMappings = await unitOfWork.ImportMappingRepository.FindAll(item => item.ImportBankId == importBank.Id);

            try
            {
                model.ImportRows = await ImportHandler.CreateImportRowsFromFile(
                    unitOfWork,
                    fileInputStream: fileStream,
                    userId: bankAccount.UserId,
                    importMappings: importMappings,
                    importBank: importBank,
                    bankAccountId: bankAccount.Id);

                // Place the import model in the cache for easy acces later
                SinanceCacheHandler.Cache(key: importCacheKey,
                    contentAction: () => model,
                    slidingExpiration: false,
                    expirationTimeSpan: new TimeSpan(0, 0, 15, 0));

                return model;
            }
            catch (Exception exc)
            {
                throw new ImportFileException("Unexpected error while importing", exc);
            }
        }

        public async Task<List<ImportBankModel>> GetImportBanks()
        {
            using var unitOfWork = _unitOfWork();

            var importBankEntities = await unitOfWork.ImportBankRepository.ListAll();

            return importBankEntities.ToDto().ToList();
        }

        public async Task<(int skippedTransactions, int savedTransactions)> SaveImport(ImportModel model)
        {
            var userId = _userIdProvider.GetCurrentUserId();
            var importCacheKey = CreateImportCacheKey(userId);
            var cachedModel = SinanceCacheHandler.RetrieveCache<ImportModel>(importCacheKey);
            if (cachedModel == null)
            {
                throw new NotFoundException(nameof(ImportModel));
            }

            using var unitOfWork = _unitOfWork();
            var bankAccount = await VerifyBankAccount(model, unitOfWork);

#pragma warning disable S1854 // Unused assignments should be removed
            var skippedTransactions = model.ImportRows.Count(item => item.ExistsInDatabase || !item.Import);
            var savedTransactions = await ImportHandler.SaveImportResultToDatabase(unitOfWork: unitOfWork,
                                                                                   bankAccountId: bankAccount.Id,
                                                                                   userId: userId,
                                                                                   importRows: model.ImportRows,
                                                                                   cachedImportRows: cachedModel.ImportRows);
#pragma warning restore S1854 // Unused assignments should be removed

            // Update the current balance of bank account and refresh them
            await _bankAccountCalculationService.UpdateCurrentBalanceForBankAccount(unitOfWork, model.BankAccountId);
            await unitOfWork.SaveAsync();

            // Clear the cache entry
            SinanceCacheHandler.ClearCache(importCacheKey);

            return (skippedTransactions, savedTransactions);
        }

        private static async Task<BankAccountEntity> VerifyBankAccount(ImportModel model, IUnitOfWork unitOfWork)
        {
            var bankAccount = await unitOfWork.BankAccountRepository.FindSingle(x => x.Id == model.BankAccountId);
            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            return bankAccount;
        }

        /// <summary>
        /// Cache key for the import model
        /// </summary>
        private string CreateImportCacheKey(int userId) => string.Format(CultureInfo.CurrentCulture, $"ImportRows_{userId}");
    }
}
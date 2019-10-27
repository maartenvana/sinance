using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Handlers;
using Sinance.Business.Services.Authentication;
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
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public ImportService(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<ImportModel> CreateImportPreview(Stream fileStream, ImportModel model)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            var importCacheKey = CreateImportCacheKey(userId);
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
            var userId = await _authenticationService.GetCurrentUserId();

            var importCacheKey = CreateImportCacheKey(userId);
            var cachedModel = SinanceCacheHandler.RetrieveCache<ImportModel>(importCacheKey);
            if (cachedModel == null)
            {
                throw new NotFoundException(nameof(ImportModel));
            }

            using var unitOfWork = _unitOfWork();
            var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == model.BankAccountId && x.UserId == userId);
            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            var skippedTransactions = model.ImportRows.Count(item => item.ExistsInDatabase || !item.Import);
            var savedTransactions = await ImportHandler.SaveImportResultToDatabase(unitOfWork,
                bankAccountId: bankAccount.Id,
                importRows: model.ImportRows,
                cachedImportRows: cachedModel.ImportRows,
                userId: bankAccount.UserId);

            // Update the current balance of bank account and refresh them
            bankAccount.CurrentBalance = await TransactionHandler.CalculateCurrentBalanceForBankAccount(unitOfWork,
                bankAccount);

            await unitOfWork.SaveAsync();

            // Clear the cache entry
            SinanceCacheHandler.ClearCache(importCacheKey);

            return (skippedTransactions, savedTransactions);
        }

        /// <summary>
        /// Cache key for the import model
        /// </summary>
        private string CreateImportCacheKey(int userId) => string.Format(CultureInfo.CurrentCulture, $"ImportRows_{userId}");
    }
}
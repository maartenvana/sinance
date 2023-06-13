using Microsoft.EntityFrameworkCore;
using Sinance.Business.Calculations;
using Sinance.Business.Exceptions;
using Sinance.Business.Handlers;
using Sinance.Communication.Model.Import;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.Imports;

public class ImportService : IImportService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly IUserIdProvider _userIdProvider;
    private readonly IBankFileImportHandler _bankFileImportHandler;

    public ImportService(
        IDbContextFactory<SinanceContext> dbContextFactory,
        IUserIdProvider userIdProvider,
        IBankFileImportHandler bankFileImportHandler)
    {
        _dbContextFactory = dbContextFactory;
        _userIdProvider = userIdProvider;
        _bankFileImportHandler = bankFileImportHandler;
    }

    public async Task<ImportModel> CreateImportPreview(Stream fileStream, ImportModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var importCacheKey = CreateImportCacheKey(_userIdProvider.GetCurrentUserId());
        SinanceCacheHandler.ClearCache(importCacheKey);

        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == model.BankAccountId);
        if (bankAccount == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        try
        {
            model.ImportRows = await _bankFileImportHandler.CreateImportRowsFromFile(
                context,
                fileInputStream: fileStream,
                userId: bankAccount.UserId,
                fileImporterId: model.BankFileImporterId,
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

    public async Task<(int skippedTransactions, int savedTransactions)> SaveImport(ImportModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var userId = _userIdProvider.GetCurrentUserId();
        var importCacheKey = CreateImportCacheKey(userId);
        var cachedModel = SinanceCacheHandler.RetrieveCache<ImportModel>(importCacheKey);
        if (cachedModel == null)
        {
            throw new NotFoundException(nameof(ImportModel));
        }

        var bankAccount = await VerifyBankAccount(model, context);

        var skippedTransactions = model.ImportRows.Count(item => item.ExistsInDatabase || !item.Import);
        var savedTransactions = await BankFileImportHandler.SaveImportResultToDatabase(context,
            bankAccountId: bankAccount.Id,
            userId: userId,
            importRows: model.ImportRows,
            cachedImportRows: cachedModel.ImportRows);

        // Update the current balance of bank account and refresh them
        await BankAccountCalculations.UpdateCurrentBalanceForBankAccount(context, model.BankAccountId);
        await context.SaveChangesAsync();

        // Clear the cache entry
        SinanceCacheHandler.ClearCache(importCacheKey);

        return (skippedTransactions, savedTransactions);
    }

    private static async Task<BankAccountEntity> VerifyBankAccount(ImportModel model, SinanceContext context)
    {
        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == model.BankAccountId);

        if (bankAccount == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        return bankAccount;
    }

    /// <summary>
    /// Cache key for the import model
    /// </summary>
    private string CreateImportCacheKey(int userId) => string.Format(CultureInfo.CurrentCulture, $"ImportRows_{userId}");
}
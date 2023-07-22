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

    public async Task<(int skippedTransactions, int savedTransactions)> ImportTransactions(Stream fileStream, ImportModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

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

            var importResult = await SaveImport(model);

            return importResult;
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

        var bankAccount = await VerifyBankAccount(model, context);

        var skippedTransactions = model.ImportRows.Count(item => item.ExistsInDatabase || !item.Import);
        var savedTransactions = await BankFileImportHandler.SaveImportResultToDatabase(context,
            bankAccountId: bankAccount.Id,
            userId: userId,
            importRows: model.ImportRows);

        await BankAccountCalculations.UpdateCurrentBalanceForBankAccount(context, model.BankAccountId);

        await context.SaveChangesAsync();

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
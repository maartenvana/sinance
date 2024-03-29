﻿using Microsoft.EntityFrameworkCore;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Import;
using Sinance.Communication.Model.Import;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers;

public class BankFileImportHandler : IBankFileImportHandler
{
    private readonly IEnumerable<IBankFileImporter> _bankFileImporters;

    public BankFileImportHandler(IEnumerable<IBankFileImporter> bankFileImporters)
    {
        this._bankFileImporters = bankFileImporters;
    }

    public IBankFileImporter GetFileImporterForId(Guid fileImporterId)
    {
        try
        {
            return _bankFileImporters.Single(x => x.Id == fileImporterId);
        }
        catch (InvalidOperationException ex)
        {
            throw new BankFileImporterNotFoundException($"No bank file importer found for id {fileImporterId}", ex);
        }
    }

    public async Task<IList<ImportRow>> CreateImportRowsFromFile(
        SinanceContext context,
        Stream fileInputStream,
        int userId,
        Guid fileImporterId,
        int bankAccountId)
    {
        var importer = GetFileImporterForId(fileImporterId);

        var importRows = ConvertFileToImportRows(fileInputStream, importer);

        if (importRows.Any())
        {
            var existingTransactions = await GetExistingTransactionsForImportRows(context, userId, bankAccountId, importRows);

            MatchExistingTransactionsWithImportRows(importRows, existingTransactions);

            await MapCategoriesToRows(context, userId, importRows);
        }

        return importRows;
    }

    private static void MatchExistingTransactionsWithImportRows(IList<ImportRow> importRows, List<ImportTransactionEntity> existingTransactions)
    {
        foreach (var importRow in importRows)
        {
            var transactionExists = ImportRowHasExistingTransaction(existingTransactions, importRow);

            importRow.ExistsInDatabase = transactionExists;
            importRow.Import = !transactionExists;
        }
    }

    private static bool ImportRowHasExistingTransaction(List<ImportTransactionEntity> existingTransactions, ImportRow importRow) =>
        existingTransactions.Exists(transaction =>
            importRow.Transaction.Amount == transaction.Amount &&
            importRow.Transaction.Name.Equals(transaction.Name.Trim(), StringComparison.CurrentCultureIgnoreCase) &&
            importRow.Transaction.Date == transaction.Date);

    private static async Task<List<ImportTransactionEntity>> GetExistingTransactionsForImportRows(
        SinanceContext context,
        int userId,
        int bankAccountId,
        IList<ImportRow> rowsToImport)
    {
        var firstDate = rowsToImport.Min(item => item.Transaction.Date);
        var lastDate = rowsToImport.Max(item => item.Transaction.Date);

        var existingTransactions = await context.ImportTransactions
                .Where(transaction =>
                    transaction.BankAccountId == bankAccountId &&
                    transaction.UserId == userId &&
                    transaction.Date >= firstDate &&
                    transaction.Date <= lastDate)
                .ToListAsync();

        return existingTransactions;
    }

    private static async Task MapCategoriesToRows(SinanceContext context, int userId, IList<ImportRow> rowsToImport)
    {
        var categoryMappings = await context.CategoryMappings
            .Include(x => x.Category)
            .Where(item => item.Category.UserId == userId)
            .ToListAsync();

        CategoryHandler.SetTransactionCategories(transactions: rowsToImport.Select(item => item.Transaction), categoryMappings: categoryMappings);
    }

    private static IList<ImportRow> ConvertFileToImportRows(Stream fileInputStream, IBankFileImporter importer)
    {
        using var reader = new StreamReader(fileInputStream);

        return importer.CreateImport(reader.BaseStream);
    }

    public static async Task<int> SaveImportResultToDatabase(SinanceContext context, int bankAccountId, int userId, IList<ImportRow> importRows)
    {
        var savedTransactions = 0;

        var importTransactions = importRows
            .Where(item => !item.ExistsInDatabase && item.Import)
            .Select(importRow => importRows.SingleOrDefault(item => item.ImportRowId == importRow.ImportRowId))
            .Where(cachedImportRow => cachedImportRow != null)
            .Select(x => x.Transaction);

        // Select all cached import rows that have to be imported
        foreach (var importTransaction in importTransactions)
        {
            // Set the application user id and bank account id
            importTransaction.BankAccountId = bankAccountId;

            // Import into both tables, the import is only for registration and double prevention
            var importTransactionEntity = importTransaction.ToNewImportEntity(userId);
            var transactionEntity = importTransaction.ToNewEntity(userId);

            transactionEntity.ImportTransaction = importTransactionEntity;

            context.ImportTransactions.Add(importTransactionEntity);
            context.Transactions.Add(transactionEntity);

            // Count how many we inserted
            savedTransactions++;
        }
        await context.SaveChangesAsync();

        return savedTransactions;
    }
}
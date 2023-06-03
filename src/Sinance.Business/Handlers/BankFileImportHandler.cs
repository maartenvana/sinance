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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers
{
    public class BankFileImportHandler : IBankFileImportHandler
    {
        private readonly IEnumerable<IBankFileImporter> bankFileImporters;

        public BankFileImportHandler(IEnumerable<IBankFileImporter> bankFileImporters)
        {
            this.bankFileImporters = bankFileImporters;
        }

        public IBankFileImporter GetFileImporterForId(Guid fileImporterId)
        {
            try
            {
                return bankFileImporters.Single(x => x.Id == fileImporterId);
            }
            catch (InvalidOperationException ex)
            {
                throw new BankFileImporterNotFoundException($"No bank file importer found for id {fileImporterId}", ex);
            }
        }

        public async Task<IList<ImportRow>> CreateImportRowsFromFile(
            IUnitOfWork unitOfWork, 
            Stream fileInputStream, 
            int userId,
            Guid fileImporterId,
            int bankAccountId)
        {
            var importer = GetFileImporterForId(fileImporterId);

            var importRows = ConvertFileToImportRows(fileInputStream, importer);

            if (importRows.Any())
            {
                var existingTransactions = await GetExistingTransactionsForImportRows(unitOfWork, userId, bankAccountId, importRows);

                MatchExistingTransactionsWithImportRows(importRows, existingTransactions);

                await MapCategoriesToRows(unitOfWork, userId, importRows);
            }

            return importRows;
        }

        private static void MatchExistingTransactionsWithImportRows(IList<ImportRow> importRows, List<TransactionEntity> existingTransactions)
        {
            foreach (var importRow in importRows)
            {
                var transactionExists = ImportRowHasExistingTransaction(existingTransactions, importRow);

                importRow.ExistsInDatabase = transactionExists;
                importRow.Import = !transactionExists;
            }
        }

        private static bool ImportRowHasExistingTransaction(List<TransactionEntity> existingTransactions, ImportRow importRow) => 
            existingTransactions.Any(transaction => 
                importRow.Transaction.Amount == transaction.Amount &&
                importRow.Transaction.Name.Equals(transaction.Name.Trim(), StringComparison.CurrentCultureIgnoreCase) &&
                importRow.Transaction.Date == transaction.Date);

        private static async Task<List<TransactionEntity>> GetExistingTransactionsForImportRows(
            IUnitOfWork unitOfWork, 
            int userId,
            int bankAccountId,
            IList<ImportRow> rowsToImport)
        {
            var firstDate = rowsToImport.Min(item => item.Transaction.Date);
            var lastDate = rowsToImport.Max(item => item.Transaction.Date);

            var existingTransactions = await unitOfWork.TransactionRepository
                .FindAll(transaction => 
                    transaction.BankAccountId == bankAccountId &&
                    transaction.UserId == userId && 
                    transaction.Date >= firstDate && 
                    transaction.Date <= lastDate);
            return existingTransactions;
        }

        private static async Task MapCategoriesToRows(IUnitOfWork unitOfWork, int userId, IList<ImportRow> rowsToImport)
        {
            var categoryMappings = await unitOfWork.CategoryMappingRepository.FindAll(item => item.Category.UserId == userId, "Category");

            CategoryHandler.SetTransactionCategories(transactions: rowsToImport.Select(item => item.Transaction), categoryMappings: categoryMappings);
        }

        private static IList<ImportRow> ConvertFileToImportRows(Stream fileInputStream, IBankFileImporter importer)
        {
            using var reader = new StreamReader(fileInputStream);

            return importer.CreateImport(reader.BaseStream);
        }

        public async Task<int> SaveImportResultToDatabase(IUnitOfWork unitOfWork, int bankAccountId, int userId,
            IList<ImportRow> importRows, IList<ImportRow> cachedImportRows)
        {
            var savedTransactions = 0;

            // Select all cached import rows that have to be imported
            foreach (var cachedImportRow in importRows.Where(item => !item.ExistsInDatabase && item.Import)
                .Select(importRow => cachedImportRows.SingleOrDefault(item => item.ImportRowId == importRow.ImportRowId))
                .Where(cachedImportRow => cachedImportRow != null))
            {
                // Set the application user id and bank account id
                cachedImportRow.Transaction.BankAccountId = bankAccountId;

                unitOfWork.TransactionRepository.Insert(cachedImportRow.Transaction.ToNewEntity(userId));

                // Count how many we inserted
                savedTransactions++;
            }
            await unitOfWork.SaveAsync();

            return savedTransactions;
        }
    }
}
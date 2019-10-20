using CsvHelper;
using CsvHelper.Configuration;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.Import;
using Sinance.Communication.Model.Transaction;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinance.Business.Handlers
{
    /// <summary>
    /// ImportHandler for importing transactions
    /// </summary>
    public static class ImportHandler
    {
        private static readonly Regex _trimExtraWhitespaceRegex = new Regex(@"\s\s+", RegexOptions.Compiled);

        /// <summary>
        /// Process an import and converts it to a list of import rows
        /// </summary>
        /// <param name="genericRepository">Generic repository to use</param>
        /// <param name="fileInputStream">File input stream </param>
        /// <param name="userId">Id of the current user</param>
        /// <param name="importMappings">Import column mappings</param>
        /// <param name="importBank">Import bank entity</param>
        /// <param name="bankAccountId">Bank account id to use for processing</param>
        /// <returns>Created list of import rows</returns>
        public static async Task<IList<ImportRow>> CreateImportRowsFromFile(IUnitOfWork unitOfWork, Stream fileInputStream, int userId,
            IList<ImportMappingEntity> importMappings, ImportBankEntity importBank, int bankAccountId)
        {
            IList<ImportRow> importRows;
            using (var reader = new StreamReader(fileInputStream))
            {
                // Convert the raw csv file to an import rows list
                importRows = ConvertFileToImportRows(reader.BaseStream, importBank, importMappings);
            }

            if (importRows != null && importRows.Any())
            {
                var firstDate = importRows.Min(item => item.Transaction.Date);
                var lastDate = importRows.Max(item => item.Transaction.Date);

                var existingTransactions = await unitOfWork.TransactionRepository
                    .FindAll(transaction => transaction.UserId == userId && transaction.Date >= firstDate && transaction.Date <= lastDate);

                foreach (var importRow in importRows)
                {
                    var transactionExists = existingTransactions.Any(transaction => importRow.Transaction.Amount == transaction.Amount &&
                                                                                     importRow.Transaction.Name.Equals(transaction.Name.Trim(),
                                                                                         StringComparison.CurrentCultureIgnoreCase) &&
                                                                                     importRow.Transaction.Date == transaction.Date &&
                                                                                     transaction.BankAccountId == bankAccountId);

                    importRow.ExistsInDatabase = transactionExists;
                    importRow.Import = !transactionExists;
                }

                var categoryMappings = await unitOfWork.CategoryMappingRepository.FindAll(item => item.Category.UserId == userId, "Category");

                CategoryHandler.SetTransactionCategories(transactions: importRows.Select(item => item.Transaction), categoryMappings: categoryMappings);
            }

            return importRows;
        }

        /// <summary>
        /// Saves the import to the database
        /// </summary>
        /// <param name="genericRepository">Repository to use</param>
        /// <param name="bankAccountId">Bank account to import for</param>
        /// <param name="importRows">Import rows that were posted</param>
        /// <param name="cachedImportRows">The cached import rows</param>
        /// <param name="userId">User to import for</param>
        /// <returns>Amount of saved import result</returns>
        public static async Task<int> SaveImportResultToDatabase(IUnitOfWork unitOfWork, int bankAccountId,
            IList<ImportRow> importRows, IList<ImportRow> cachedImportRows, int userId)
        {
            var savedTransactions = 0;

            // Select all cached import rows that have to be imported
            foreach (var cachedImportRow in importRows.Where(item => item.ExistsInDatabase == false && item.Import)
                .Select(importRow => cachedImportRows.SingleOrDefault(item => item.ImportRowId == importRow.ImportRowId))
                .Where(cachedImportRow => cachedImportRow != null))
            {
                // Set the application user id and bank account id
                cachedImportRow.Transaction.BankAccountId = bankAccountId;

                // Remove the category entity reference, otherwise it wont save
                var mappedTransactionCategory = cachedImportRow.Transaction.Categories.FirstOrDefault();

                unitOfWork.TransactionRepository.Insert(cachedImportRow.Transaction.ToNewEntity(userId));

                // Count how many we inserted
                savedTransactions++;
            }
            await unitOfWork.SaveAsync();

            return savedTransactions;
        }

        /// <summary>
        /// Converts a file to a list of import rows
        /// </summary>
        /// <param name="fileStream">Filestream to use</param>
        /// <param name="importBank">Import bank entity with settings</param>
        /// <param name="importMappings">Import column mappings to use for mapping values</param>
        /// <returns>A created import row list</returns>
        private static IList<ImportRow> ConvertFileToImportRows(Stream fileStream, ImportBankEntity importBank, IList<ImportMappingEntity> importMappings)
        {
            IList<ImportRow> importRows = new List<ImportRow>();

            using (var streamReader = new StreamReader(fileStream))
            {
                using var csv = new CsvReader(streamReader, new Configuration
                {
                    Delimiter = importBank.Delimiter
                });
                // If the import contains a header first read this so we skip it in the loop
                if (importBank.ImportContainsHeader)
                {
                    csv.Read();
                }

                var importRowId = 0;

                // Keep going untill we are done
                while (csv.Read())
                {
                    // Create a new import row
                    var importRow = new ImportRow
                    {
                        Transaction = new TransactionModel()
                    };

                    var amountIsNegative = false;

                    // Loop through fields
                    for (var i = 0; csv.TryGetField<string>(i, out var rawValue); i++)
                    {
                        var importMapping = importMappings.SingleOrDefault(item => item.ColumnIndex == i);

                        // If we have a mapping for this column, convert it
                        if (importMapping != null)
                        {
                            if (importMapping.ColumnTypeId == ColumnType.AddSubtract)
                            {
                                amountIsNegative = ConvertRawToIsNegative(rawValue, importMapping.FormatValue);
                            }
                            else
                            {
                                ConvertRawColumn(rawValue, importMapping, importRow.Transaction);
                            }
                        }
                    }

                    if (amountIsNegative && importRow.Transaction.Amount > 0)
                    {
                        importRow.Transaction.Amount *= -1;
                    }

                    importRow.ImportRowId = importRowId++;

                    importRows.Add(importRow);
                }
            }

            return importRows;
        }

        /// <summary>
        /// Converts a raw value to a transaction value
        /// </summary>
        /// <param name="transaction">Transaction to set property of</param>
        /// <param name="importMapping">Specifies which property to map to</param>
        /// <param name="rawValue">The raw string value</param>
        private static void ConvertRawColumn(string rawValue, ImportMappingEntity importMapping, TransactionModel transaction)
        {
            // Make sure we got no leading or trailing whitespace
            rawValue = rawValue.Trim();
            rawValue = _trimExtraWhitespaceRegex.Replace(rawValue, " ");

            switch (importMapping.ColumnTypeId)
            {
                case ColumnType.Ignore:
                    break;

                case ColumnType.Date:
                    transaction.Date = ConvertRawToDate(rawValue, importMapping.FormatValue);
                    break;

                case ColumnType.Amount:
                    transaction.Amount = ConvertRawToAmount(rawValue);
                    break;

                case ColumnType.Description:
                    transaction.Description = rawValue;
                    break;

                case ColumnType.Name:
                    transaction.Name = rawValue;
                    break;

                case ColumnType.DestinationAccount:
                    transaction.DestinationAccount = rawValue;
                    break;

                case ColumnType.BankAccountFrom:
                    transaction.FromAccount = rawValue;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Converts a raw value to a decimal
        /// </summary>
        /// <param name="rawValue">Raw value to convert</param>
        /// <returns>Converted decimal</returns>
        private static decimal ConvertRawToAmount(string rawValue)
        {
            if (rawValue.Count(item => item == '.') > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rawValue));
            }

            rawValue = rawValue.Replace(',', '.');
            var amount = decimal.Parse(rawValue, CultureInfo.InvariantCulture);
            return amount;
        }

        /// <summary>
        /// Converts a string to DateTime
        /// </summary>
        /// <param name="rawValue">Raw string value</param>
        /// <param name="format">Format of the date</param>
        /// <returns>Converted DateTime</returns>
        private static DateTime ConvertRawToDate(string rawValue, string format)
        {
            try
            {
                var date = DateTime.ParseExact(rawValue, format, CultureInfo.InvariantCulture);
                return date;
            }
            catch (Exception exception)
            {
                throw new ArgumentOutOfRangeException("rawValue has an incorrect format", exception);
            }
        }

        /// <summary>
        /// Converts a raw IsNegative value to a boolean IsNegative value
        /// </summary>
        /// <param name="rawValue">Raw IsNegative value</param>
        /// <param name="format">What value the rawvalue must contain to be negative</param>
        /// <returns>Whether or not it is negative</returns>
        private static bool ConvertRawToIsNegative(string rawValue, string format)
        {
            var isNegative = string.Equals(rawValue, format, StringComparison.OrdinalIgnoreCase);

            return isNegative;
        }
    }
}
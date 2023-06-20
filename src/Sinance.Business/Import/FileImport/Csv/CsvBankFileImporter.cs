using CsvHelper;
using CsvHelper.Configuration;
using Sinance.Communication.Model.Import;
using Sinance.Communication.Model.Transaction;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sinance.Business.Import.FileImport.Csv;

public class CsvBankFileImporter
{
    private static readonly Regex _trimExtraWhitespaceRegex = new Regex(@"\s\s+", RegexOptions.Compiled, matchTimeout: TimeSpan.FromSeconds(30));

    private string delimiter;

    private readonly bool importContainsHeader;

    private readonly List<CsvBankFileColumnMapping> columnMappings;

    public CsvBankFileImporter(string delimiter, bool importContainsHeader, List<CsvBankFileColumnMapping> columnMappings)
    {
        this.delimiter = delimiter;
        this.importContainsHeader = importContainsHeader;
        this.columnMappings = columnMappings;
    }

    public IList<ImportRow> CreateImport(Stream fileStream)
    {
        IList<ImportRow> importRows = new List<ImportRow>();

        using (var streamReader = new StreamReader(fileStream))
        {
            using var csv = new CsvReader(streamReader, new CsvConfiguration(cultureInfo: CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter
            });

            // If the import contains a header first read this so we skip it in the loop
            if (importContainsHeader)
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
                    var columnMapping = columnMappings.SingleOrDefault(item => item.ColumnIndex == i);

                    // If we have a mapping for this column, convert it
                    if (columnMapping != null)
                    {
                        if (columnMapping.ColumnTypeId == ColumnType.AddSubtract)
                        {
                            amountIsNegative = ConvertRawToIsNegative(rawValue, columnMapping.FormatValue);
                        }
                        else
                        {
                            ConvertRawColumn(rawValue, columnMapping, importRow.Transaction);
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
    private static void ConvertRawColumn(string rawValue, CsvBankFileColumnMapping columnMapping, TransactionModel transaction)
    {
        // Make sure we got no leading or trailing whitespace
        rawValue = rawValue.Trim();
        rawValue = _trimExtraWhitespaceRegex.Replace(rawValue, " ");

        switch (columnMapping.ColumnTypeId)
        {
            case ColumnType.Ignore:
                break;

            case ColumnType.Date:
                transaction.Date = ConvertRawToDate(rawValue, columnMapping.FormatValue);
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
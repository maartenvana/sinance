using Sinance.Business.Import.FileImport.Csv;
using Sinance.Communication.Model.Import;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sinance.Business.Import;

public class AbnAmroTxtFileImporter : IBankFileImporter
{
    public Guid Id => Guid.Parse("61b5e3a9-2c2e-4c02-a302-5cc1b1a8f5f2");

    public string FriendlyName => "ABN Amro TXT";

    public IList<ImportRow> CreateImport(Stream fileStream)
    {
        var importer = CreateImporter();

        return importer.CreateImport(fileStream);
    }

    private CsvBankFileImporter CreateImporter() =>
        new CsvBankFileImporter(delimiter: "\t", importContainsHeader: false, columnMappings: GetColumnMappings());

    private List<CsvBankFileColumnMapping> GetColumnMappings() => new List<CsvBankFileColumnMapping>
    {
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 0,
            ColumnName = "Rekening",
            ColumnTypeId = ColumnType.BankAccountFrom,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 1,
            ColumnName = "Naam (eur)",
            ColumnTypeId = ColumnType.Name,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 2,
            ColumnName = "Datum",
            ColumnTypeId = ColumnType.Date,
            FormatValue = "yyyyMMdd"
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 6,
            ColumnName = "Bedrag",
            ColumnTypeId = ColumnType.Amount,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 7,
            ColumnName = "Mededelingen",
            ColumnTypeId = ColumnType.Description,
            FormatValue = "Af"
        }
    };
}
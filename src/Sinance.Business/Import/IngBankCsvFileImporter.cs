using Sinance.Business.Import.FileImport.Csv;
using Sinance.Communication.Model.Import;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sinance.Business.Import;

public class IngBankCsvFileImporter : IBankFileImporter
{
    public Guid Id => Guid.Parse("f9f1508c-9998-4f3d-96d9-bab3547e9922");

    public string FriendlyName => "ING Bank CSV";

    public IList<ImportRow> CreateImport(Stream fileStream)
    {
        var importer = CreateImporter();

        return importer.CreateImport(fileStream);
    }

    private CsvBankFileImporter CreateImporter() => 
        new CsvBankFileImporter(delimiter: ";", importContainsHeader: true, columnMappings: GetColumnMappings());

    private List<CsvBankFileColumnMapping> GetColumnMappings() => new List<CsvBankFileColumnMapping>
    {
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 0,
            ColumnName = "Datum",
            ColumnTypeId = ColumnType.Date,
            FormatValue = "yyyyMMdd"
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 1,
            ColumnName = "Naam / Omschrijving",
            ColumnTypeId = ColumnType.Name,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 2,
            ColumnName = "Rekening",
            ColumnTypeId = ColumnType.BankAccountFrom,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 3,
            ColumnName = "Tegenrekening",
            ColumnTypeId = ColumnType.DestinationAccount,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 5,
            ColumnName = "Af Bij",
            ColumnTypeId = ColumnType.AddSubtract,
            FormatValue = "Af"
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
            ColumnIndex = 8,
            ColumnName = "Mededelingen",
            ColumnTypeId = ColumnType.Description,
            FormatValue = null
        }
    };
}

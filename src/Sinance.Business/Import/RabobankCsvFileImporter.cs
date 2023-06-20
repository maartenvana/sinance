using Sinance.Business.Import.FileImport.Csv;
using Sinance.Communication.Model.Import;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sinance.Business.Import;

public class RabobankCsvFileImporter : IBankFileImporter
{
    public Guid Id => Guid.Parse("7e69d969-cb2a-49a8-939f-0e6cfdf2cfbe");

    public string FriendlyName => "Rabobank CSV";

    public IList<ImportRow> CreateImport(Stream fileStream)
    {
        var importer = CreateImporter();

        return importer.CreateImport(fileStream);
    }

    private CsvBankFileImporter CreateImporter() =>
        new CsvBankFileImporter(delimiter: ",", importContainsHeader: false, columnMappings: GetColumnMappings());

    private List<CsvBankFileColumnMapping> GetColumnMappings() => new List<CsvBankFileColumnMapping>
    {
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 0,
            ColumnName = "Datum",
            ColumnTypeId = Communication.Model.Import.ColumnType.Date,
            FormatValue = "yyyyMMdd"
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 1,
            ColumnName = "Naam/omschrijving",
            ColumnTypeId = Communication.Model.Import.ColumnType.Name,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 2,
            ColumnName = "Rekening",
            ColumnTypeId = Communication.Model.Import.ColumnType.BankAccountFrom,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 3,
            ColumnName = "Tegenrekening",
            ColumnTypeId = Communication.Model.Import.ColumnType.DestinationAccount,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 5,
            ColumnName = "Af Bij",
            ColumnTypeId = Communication.Model.Import.ColumnType.AddSubtract,
            FormatValue = "D"
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 6,
            ColumnName = "Bedrag",
            ColumnTypeId = Communication.Model.Import.ColumnType.Amount,
            FormatValue = null
        },
        new CsvBankFileColumnMapping
        {
            ColumnIndex = 8,
            ColumnName = "Mededelingen",
            ColumnTypeId = Communication.Model.Import.ColumnType.Description,
            FormatValue = null
        }
    };
}
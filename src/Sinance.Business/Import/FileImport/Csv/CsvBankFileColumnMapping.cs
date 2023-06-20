using Sinance.Communication.Model.Import;

namespace Sinance.Business.Import.FileImport.Csv;

public class CsvBankFileColumnMapping
{
    public int ColumnIndex { get; set; }

    public string ColumnName { get; set; }

    public ColumnType ColumnTypeId { get; set; }

    public string FormatValue { get; set; }
}
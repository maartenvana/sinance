namespace Sinance.BlazorApp.Model;

public class SelectableTableRow<TRowDataType> where TRowDataType : class
{
    public string RowId { get; set; }

    public TRowDataType RowData { get; set; }

    public bool IsSelected { get; set; }
}

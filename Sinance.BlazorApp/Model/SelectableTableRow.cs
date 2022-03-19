namespace Sinance.BlazorApp.Model
{
    public class SelectableTableRow<T> where T : class
    {
        public T RowData { get; set; }

        public bool IsSelected { get; set; }
    }
}

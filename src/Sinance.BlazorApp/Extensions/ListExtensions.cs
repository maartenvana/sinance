using Sinance.BlazorApp.Model;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<SelectableTableRow<T>> ToSelectableTableRows<T>(this IEnumerable<T> enumerable) where T : class =>
            enumerable.Select(x => x.ToSelectableTableRow());

        private static SelectableTableRow<T> ToSelectableTableRow<T>(this T Data) where T : class =>
            new()
            {
                RowData = Data,
                IsSelected = false
            };

    }
}

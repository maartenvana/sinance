using Sinance.BlazorApp.Business.Model.Category;

namespace Sinance.BlazorApp.Business.Model.Transaction
{
    public class SearchTransactionsFilterModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int? BankAccountId { get; set; }

        public int? CategoryId { get; set; } = CategoryModel.All.Id;

        public int Page { get; set; }

        public int PageSize { get; set; } = SinanceDefaults.TransactionPageSize;
    }
}

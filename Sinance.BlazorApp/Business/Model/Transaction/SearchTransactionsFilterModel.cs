using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Model.Transaction
{
    public class SearchTransactionsFilterModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int? BankAccountId { get; set; }

        public int? Category { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}

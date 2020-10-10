using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Model.Transaction
{
    public class SearchTransactionsFilterModel
    {
        public int? BankAccountId { get; set; }

        public List<int> Categories { get; set; } = new List<int>();

        public bool NoCategory { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}

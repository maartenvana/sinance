using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.BlazorApp.Business.Model.Transaction
{
    public class TransactionModel
    {
        public string Description { get; internal set; }
        public string Name { get; internal set; }
        public decimal Amount { get; internal set; }
        public DateTime Date { get; internal set; }
        public string CategoryName { get; internal set; }
    }
}

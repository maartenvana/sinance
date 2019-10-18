using System;
using System.Collections.Generic;
using System.Text;

namespace Sinance.Communication.Model.Transaction
{
    public class TransactionCategoryModel
    {
        /// <summary>
        /// Amount
        /// </summary>
        public decimal? Amount { get; set; }

        public int CategoryId { get; set; }
        public string ColorCode { get; set; }
        public string Name { get; set; }
    }
}
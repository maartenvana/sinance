using Microsoft.AspNetCore.Mvc.Rendering;
using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.Web.Model
{
    public class EditTransactionViewModel
    {
        public List<SelectListItem> AvailableCategories { get; set; }
        public TransactionModel Transaction { get; set; }
    }
}
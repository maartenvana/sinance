using Sinance.Communication.Model.BankAccount;
using Sinance.Communication.Model.Category;
using Sinance.Communication.Model.Transaction;
using System.Collections.Generic;

namespace Sinance.Web.Model;

/// <summary>
/// Account overview model
/// </summary>
public class AccountOverviewViewModel
{
    /// <summary>
    /// Bank account to display
    /// </summary>
    public BankAccountModel Account { get; set; }

    /// <summary>
    /// Total current balance of the account
    /// </summary>
    public decimal AccountBalance { get; set; }

    /// <summary>
    /// Available categories to choose from for quick editing a category of a transaction
    /// </summary>
    public IEnumerable<CategoryModel> AvailableCategories { get; set; }

    /// <summary>
    /// Transaction for this bank account
    /// </summary>
    public IEnumerable<TransactionModel> Transactions { get; set; }
}
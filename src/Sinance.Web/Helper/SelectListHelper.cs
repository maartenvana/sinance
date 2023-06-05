using Microsoft.AspNetCore.Mvc.Rendering;
using Sinance.Communication.Model.BankAccount;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sinance.Web.Helper;

/// <summary>
/// Select list helper for creating select lists for dropdowns
/// </summary>
public static class SelectListHelper
{
    /// <summary>
    /// Creates a list of bank accounts for a dropdown
    /// </summary>
    /// <returns>Created list of bank accounts</returns>
    public static IEnumerable<SelectListItem> CreateActiveBankAccountSelectList(IEnumerable<BankAccountModel> bankAccounts, BankAccountType? typeFilter)
    {
        var availableAccounts = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = Resources.ChooseBankAccount,
                    Value = "0"
                }
            };

        var selectedBankAccounts = typeFilter.HasValue ? bankAccounts.Where(item => item.AccountType == typeFilter.Value) : bankAccounts;
        selectedBankAccounts = selectedBankAccounts.Where(item => !item.Disabled);

        availableAccounts.AddRange(selectedBankAccounts.Select(item => new SelectListItem
        {
            Text = item.Name,
            Value = item.Id.ToString(CultureInfo.InvariantCulture)
        }));

        return availableAccounts;
    }

    /// <summary>
    /// Creates a select list for all months in a year
    /// </summary>
    /// <returns>A select list containing all months</returns>
    public static SelectList CreateMonthSelectList()
    {
        var availableMonths = new List<SelectListItem>
        {
            new SelectListItem { Text = Resources.January, Value = "1" },
            new SelectListItem { Text = Resources.February, Value = "2" },
            new SelectListItem { Text = Resources.March, Value = "3" },
            new SelectListItem { Text = Resources.April, Value = "4" },
            new SelectListItem { Text = Resources.May, Value = "5" },
            new SelectListItem { Text = Resources.June, Value = "6" },
            new SelectListItem { Text = Resources.July, Value = "7" },
            new SelectListItem { Text = Resources.August, Value = "8" },
            new SelectListItem { Text = Resources.September, Value = "9" },
            new SelectListItem { Text = Resources.October, Value = "10" },
            new SelectListItem { Text = Resources.November, Value = "11" },
            new SelectListItem { Text = Resources.December, Value = "12" }
        };

        var selectList = new SelectList(availableMonths, "Value", "Text", DateTime.Now.Month.ToString(CultureInfo.InvariantCulture));

        return selectList;
    }

    /// <summary>
    /// Creates a select list containing the current year
    /// </summary>
    /// <returns>Select list containing the years</returns>
    public static IList<SelectListItem> CreateYearSelectList()
    {
        var availableYears = new List<SelectListItem>();

        for (var i = DateTime.Now.Year - 5; i < DateTime.Now.Year; i++)
        {
            availableYears.Add(new SelectListItem { Text = i.ToString(CultureInfo.InvariantCulture), Value = i.ToString(CultureInfo.InvariantCulture), Selected = i == DateTime.Now.Year });
        }

        return availableYears;
    }
}
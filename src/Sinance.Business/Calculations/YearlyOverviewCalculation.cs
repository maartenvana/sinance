using Sinance.Business.Calculations.Subcalculations;
using Sinance.Business.Constants;
using Sinance.Business.Services.BankAccounts;
using Sinance.Business.Services.Categories;
using Sinance.Business.Services.Transactions;
using Sinance.Communication.Model.BankAccount;
using Sinance.Communication.Model.Shared;
using Sinance.Communication.Model.StandardReport.Yearly;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public class YearlyOverviewCalculation : IYearlyOverviewCalculation
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly IUnitOfWork _unitOfWork;

    public YearlyOverviewCalculation(IUnitOfWork unitOfWork,
        ICategoryService categoryService,
        IBankAccountService bankAccountService,
        ITransactionService transactionService)
    {
        _unitOfWork = unitOfWork;
        _categoryService = categoryService;
        _bankAccountService = bankAccountService;
        _transactionService = transactionService;
    }

    public async Task<YearlyOverviewModel> CalculateForYear(int year)
    {
        var result = new YearlyOverviewModel
        {
            Year = year
        };

        var startYearDate = new DateTime(year, 01, 01);
        var nextYearDate = new DateTime(year + 1, 01, 01);

        

        var totalStartBalance = await BalanceCalculations.TotalBalanceBeforeDate(_unitOfWork, startYearDate);
        var totalEndBalance = await BalanceCalculations.TotalBalanceBeforeDate(_unitOfWork, nextYearDate);

        result.TotalBalance = new YearBalance(totalStartBalance, totalEndBalance);

        var bankAccounts = await _bankAccountService.GetActiveBankAccountsForCurrentUser();
        var totalPerBankAccountType = new Dictionary<BankAccountType, YearAmountAndPercentage>();
        foreach (var bankAccount in bankAccounts)
        {
            var bankAccountStartBalance = await BalanceCalculations.TotalBalanceForBankAccountBeforeDate(_unitOfWork, startYearDate, bankAccount);
            var bankAccountEndBalance = await BalanceCalculations.TotalBalanceForBankAccountBeforeDate(_unitOfWork, nextYearDate, bankAccount);

            result.BalancePerBankAccount.Add(bankAccount, new YearBalance(bankAccountStartBalance, bankAccountEndBalance));

            if (!totalPerBankAccountType.ContainsKey(bankAccount.AccountType))
            {
                totalPerBankAccountType.Add(bankAccount.AccountType, new YearAmountAndPercentage(
                    start: new AmountAndPercentage(bankAccountStartBalance, bankAccountStartBalance / totalStartBalance * 100),
                    end: new AmountAndPercentage(bankAccountEndBalance, bankAccountEndBalance / totalEndBalance * 100))
                );
            }
            else
            {
                totalPerBankAccountType[bankAccount.AccountType].Start.Amount += bankAccountStartBalance;
                totalPerBankAccountType[bankAccount.AccountType].Start.Percentage = totalPerBankAccountType[bankAccount.AccountType].Start.Amount / totalStartBalance * 100;

                totalPerBankAccountType[bankAccount.AccountType].End.Amount += bankAccountEndBalance;
                totalPerBankAccountType[bankAccount.AccountType].End.Percentage = totalPerBankAccountType[bankAccount.AccountType].End.Amount / totalEndBalance * 100;
            }
        }

        result.BalancePerBankAccountType = totalPerBankAccountType;

        var allCategories = await _categoryService.GetAllCategoriesForCurrentUser();
        var internalCashFlowCategory = allCategories.Single(x => x.Name == StandardCategoryNames.InternalCashFlowName);

        var biggestExpenses = await _transactionService.GetBiggestExpensesForYearForCurrentUser(year, count: 20, skip: 0, excludeCategoryIds: new int[] {
            internalCashFlowCategory.Id
        });

        result.BiggestExpenses = biggestExpenses.OrderBy(x => x.Amount).ThenByDescending(x => x.Date).ToList();

        return result;
    }
}
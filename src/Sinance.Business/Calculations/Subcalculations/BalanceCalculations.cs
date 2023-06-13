using Microsoft.EntityFrameworkCore;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations.Subcalculations;

public static class BalanceCalculations
{
    public static async Task<decimal> TotalBalanceBeforeDate(SinanceContext context, DateTime date)
    {
        var totalStartBalance = await context.BankAccounts.Where(x => !x.Disabled).SumAsync(x => x.StartBalance);
        var totalTransactionBalance = await context.Transactions.Where(x => x.Date < date && !x.BankAccount.Disabled).SumAsync(x => x.Amount);

        return totalStartBalance + totalTransactionBalance;
    }

    public static async Task<decimal> TotalBalanceForBankAccountBeforeDate(SinanceContext context, DateTime date, BankAccountModel bankAccount)
    {
        var totalTransactionBalance = await context.Transactions
            .Where(x => x.BankAccountId == bankAccount.Id && x.Date < date)
            .SumAsync(x => x.Amount);

        return bankAccount.StartBalance + totalTransactionBalance;
    }
}
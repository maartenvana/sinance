using Microsoft.EntityFrameworkCore;
using Sinance.Business.Exceptions;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Calculations;

public static class BankAccountCalculations
{
    public static async Task<decimal> CalculateCurrentBalanceForBankAccount(SinanceContext context, BankAccountEntity bankAccount)
    {
        var transactionSum = await context.Transactions.Where(x => x.BankAccountId == bankAccount.Id).SumAsync(x => x.Amount);

        return bankAccount.StartBalance + transactionSum;
    }

    public static async Task UpdateCurrentBalanceForBankAccount(SinanceContext context, int bankAccountId)
    {
        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == bankAccountId);

        if (bankAccount == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        bankAccount.CurrentBalance = await CalculateCurrentBalanceForBankAccount(context, bankAccount);
    }
}
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;

namespace Sinance.BlazorApp.Business.Services;

public class BankAccountService : IBankAccountService
{
    private readonly ISinanceDbContextFactory<SinanceContext> dbContextFactory;

    public BankAccountService(ISinanceDbContextFactory<SinanceContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public List<BankAccountModel> GetAllBankAccounts()
    {
        using var context = dbContextFactory.CreateDbContext();

        var bankAccountEntities = context.BankAccounts.ToList();

        return bankAccountEntities.ToDto().ToList();
    }

    public List<BankAccountModel> GetAllActiveBankAccounts()
    {
        using var context = dbContextFactory.CreateDbContext();

        var bankAccountEntities = context.BankAccounts.Where(x => x.Disabled == false).ToList();

        return bankAccountEntities.ToDto().ToList();
    }

    public async Task RecalculateBalanceAsync(int bankAccountId)
    {
        using var context = dbContextFactory.CreateDbContext();

        var bankAccount = context.BankAccounts.Single(x => x.Id == bankAccountId);
        var bankAccountTransactionsSum = context.Transactions.Where(x => x.BankAccountId == bankAccountId).Sum(x => x.Amount);

        bankAccount.CurrentBalance = bankAccount.StartBalance + bankAccountTransactionsSum;

        await context.SaveChangesAsync();
    }

    public BankAccountModel GetBankAccount(int id)
    {
        using var context = dbContextFactory.CreateDbContext();

        var bankAccountEntity = context.BankAccounts.Single(x => x.Id == id);

        return bankAccountEntity.ToDto();
    }
}

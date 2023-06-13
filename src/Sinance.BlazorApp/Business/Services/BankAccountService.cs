using Microsoft.EntityFrameworkCore;
using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.Storage;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;

    public BankAccountService(IDbContextFactory<SinanceContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public List<BankAccountModel> GetAllBankAccounts()
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccountEntities = context.BankAccounts.ToList();

        return bankAccountEntities.ToDto().ToList();
    }

    public List<BankAccountModel> GetAllActiveBankAccounts()
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccountEntities = context.BankAccounts.Where(x => !x.Disabled).ToList();

        return bankAccountEntities.ToDto().ToList();
    }
}

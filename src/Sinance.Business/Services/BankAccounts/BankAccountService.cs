using Microsoft.EntityFrameworkCore;
using Sinance.Business.Calculations;
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts;

public class BankAccountService : IBankAccountService
{
    private readonly IDbContextFactory<SinanceContext> _dbContextFactory;
    private readonly IUserIdProvider _userIdProvider;

    public BankAccountService(
        IDbContextFactory<SinanceContext> dbContextFactory,
        IUserIdProvider userIdProvider)
    {
        _dbContextFactory = dbContextFactory;
        _userIdProvider = userIdProvider;
    }

    public async Task<BankAccountModel> CreateBankAccountForCurrentUser(BankAccountModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Name == model.Name);

        if (bankAccount != null)
            throw new AlreadyExistsException(nameof(BankAccountEntity));

        var bankAccountEntity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());
        await context.BankAccounts.AddAsync(bankAccountEntity);
        await context.SaveChangesAsync();

        return bankAccountEntity.ToDto();
    }

    public async Task DeleteBankAccountByIdForCurrentUser(int accountId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == accountId);

        if (bankAccount == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        context.BankAccounts.Remove(bankAccount);

        await context.SaveChangesAsync();
    }

    public async Task<List<BankAccountModel>> GetActiveBankAccountsForCurrentUser()
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccounts = await context.BankAccounts.Where(x => !x.Disabled)
            .OrderBy(x => x.Name)
            .ToListAsync();

        return bankAccounts.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<BankAccountModel>> GetAllBankAccountsForCurrentUser()
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccounts = await context.BankAccounts
            .OrderBy(x => x.Name)
            .ToListAsync();

        return bankAccounts.Select(x => x.ToDto()).ToList();
    }

    public async Task<BankAccountModel> GetBankAccountByIdForCurrentUser(int bankAccountId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccount = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == bankAccountId);

        if (bankAccount == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        return bankAccount.ToDto();
    }

    public async Task<BankAccountModel> UpdateBankAccountForCurrentUser(BankAccountModel model)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var bankAccountEntity = await context.BankAccounts.SingleOrDefaultAsync(x => x.Id == model.Id);

        if (bankAccountEntity == null)
            throw new NotFoundException(nameof(BankAccountEntity));

        var recalculateCurrentBalance = bankAccountEntity.StartBalance != model.StartBalance;

        bankAccountEntity.UpdateFromModel(model);

        if (recalculateCurrentBalance)
            bankAccountEntity.CurrentBalance = await BankAccountCalculations.CalculateCurrentBalanceForBankAccount(context, bankAccountEntity);

        await context.SaveChangesAsync();

        return bankAccountEntity.ToDto();
    }
}
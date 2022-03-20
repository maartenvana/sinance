using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using System.Collections.Generic;
using System.Linq;

namespace Sinance.BlazorApp.Business.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IDbContextFactory<SinanceContext> dbContextFactory;

        public BankAccountService(IDbContextFactory<SinanceContext> dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public List<BankAccountModel> GetAllBankAccounts()
        {
            using var context = this.dbContextFactory.CreateDbContext();

            var bankAccountEntities = context.BankAccounts.ToList();

            return bankAccountEntities.ToDto().ToList();
        }

        public List<BankAccountModel> GetAllActiveBankAccounts()
        {
            using var context = this.dbContextFactory.CreateDbContext();

            var bankAccountEntities = context.BankAccounts.Where(x => x.Disabled == false).ToList();

            return bankAccountEntities.ToDto().ToList();
        }

        public BankAccountModel GetBankAccount(int id)
        {
            using var context = this.dbContextFactory.CreateDbContext();

            var bankAccountEntity = context.BankAccounts.Single(x => x.Id == id);

            return bankAccountEntity.ToDto();
        }
    }
}

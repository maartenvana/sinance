using Sinance.BlazorApp.Business.Extensions;
using Sinance.BlazorApp.Business.Model.BankAccount;
using Sinance.BlazorApp.Storage;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}

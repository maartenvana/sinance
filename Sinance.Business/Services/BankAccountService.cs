using Sinance.Domain.Entities;
using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Business.Services.Authentication;

namespace Sinance.Business.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IAuthenticationService _sessionService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BankAccountService(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService sessionService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        public async Task<IList<BankAccount>> GetActiveBankAccountsForCurrentUser()
        {
            var userId = await _sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                IList<BankAccount> bankAccounts = unitOfWork.BankAccountRepository
                    .FindAll(item => item.UserId == userId && !item.Disabled)
                    .OrderBy(item => item.Name)
                    .ToList();

                return bankAccounts;
            }
        }

        /// <summary>
        /// Bank accounts active in this session
        /// </summary>
        public async Task<IList<BankAccount>> GetAllBankAccountsForCurrentUser()
        {
            var userId = await this._sessionService.GetCurrentUserId();

            using (var unitOfWork = _unitOfWork())
            {
                IList<BankAccount> bankAccounts = unitOfWork.BankAccountRepository
                .FindAll(item => item.UserId == userId)
                .OrderBy(item => item.Name)
                .ToList();

                return bankAccounts;
            }
        }
    }
}
using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts
{
    internal class BankAccountService : IBankAccountService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IBankAccountCalculationService _bankAccountCalculationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        internal BankAccountService(
            Func<IUnitOfWork> unitOfWork,
            IBankAccountCalculationService bankAccountCalculationService,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _bankAccountCalculationService = bankAccountCalculationService;
            _authenticationService = authenticationService;
        }

        public async Task<BankAccountModel> CreateBankAccountForCurrentUser(BankAccountModel model)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var bankAccount = await unitOfWork.BankAccountRepository.FindSingle(x => x.Name == model.Name && x.UserId == userId);

            if (bankAccount != null)
            {
                throw new AlreadyExistsException(nameof(BankAccountEntity));
            }

            var bankAccountEntity = model.ToNewEntity(userId);
            unitOfWork.BankAccountRepository.Insert(bankAccountEntity);
            await unitOfWork.SaveAsync();

            return bankAccountEntity.ToDto();
        }

        public async Task DeleteBankAccountByIdForCurrentUser(int accountId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == accountId && x.UserId == userId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            unitOfWork.BankAccountRepository.Delete(bankAccount);

            await unitOfWork.SaveAsync();
        }

        public async Task<List<BankAccountModel>> GetActiveBankAccountsForCurrentUser()
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var bankAccounts = (await unitOfWork.BankAccountRepository
                .FindAll(x => x.UserId == userId && !x.Disabled))
                .OrderBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToList();

            return bankAccounts;
        }

        public async Task<List<BankAccountModel>> GetAllBankAccountsForCurrentUser()
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();
            var bankAccounts = (await unitOfWork.BankAccountRepository
                .FindAll(x => x.UserId == userId))
                .OrderBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToList();

            return bankAccounts;
        }

        public async Task<BankAccountModel> GetBankAccountByIdForCurrentUser(int bankAccountId)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingle(x => x.UserId == userId && x.Id == bankAccountId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            return bankAccount.ToDto();
        }

        public async Task<BankAccountModel> UpdateBankAccountForCurrentUser(BankAccountModel model)
        {
            var userId = await _authenticationService.GetCurrentUserId();

            using var unitOfWork = _unitOfWork();

            var bankAccountEntity = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.UserId == userId && x.Id == model.Id);

            if (bankAccountEntity == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            var recalculateCurrentBalance = bankAccountEntity.StartBalance != model.StartBalance;

            bankAccountEntity.UpdateFromModel(model);
            
            if (recalculateCurrentBalance)
            {
                bankAccountEntity.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(unitOfWork, bankAccountEntity);
            }

            unitOfWork.BankAccountRepository.Update(bankAccountEntity);
            await unitOfWork.SaveAsync();

            return bankAccountEntity.ToDto();
        }
    }
}
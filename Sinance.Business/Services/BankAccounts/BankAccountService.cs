using Sinance.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sinance.Business.Services.Authentication;
using Sinance.Communication.BankAccount;
using Sinance.Business.Extensions;
using Sinance.Business.Exceptions;
using Sinance.Storage.Entities;
using Sinance.Business.Handlers;

namespace Sinance.Business.Services.BankAccounts
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly Func<IUnitOfWork> _unitOfWork;

        public BankAccountService(
            Func<IUnitOfWork> unitOfWork,
            IAuthenticationService authenticationService)
        {
            _unitOfWork = unitOfWork;
            _authenticationService = authenticationService;
        }

        public async Task<BankAccountModel> CreateBankAccountForCurrentUser(int userId, BankAccountModel model)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccounts = await unitOfWork.BankAccountRepository.FindAll(x => x.Name == model.Name && x.UserId == userId);

            if (!bankAccounts.Any())
            {
                var bankAccountEntity = model.ToNewEntity(userId);
                unitOfWork.BankAccountRepository.Insert(bankAccountEntity);
                await unitOfWork.SaveAsync();

                return bankAccountEntity.ToDto();
            }
            else
            {
                throw new AlreadyExistsException(nameof(BankAccountEntity));
            }
        }

        public async Task DeleteBankAccountByIdForUser(int userId, int accountId)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == accountId && x.UserId == userId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            unitOfWork.BankAccountRepository.Delete(bankAccount);

            await unitOfWork.SaveAsync();
        }

        public async Task<IList<BankAccountModel>> GetActiveBankAccountsForUser(int userId)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccounts = (await unitOfWork.BankAccountRepository
                .FindAll(x => x.UserId == userId && !x.Disabled))
                .OrderBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToList();

            return bankAccounts;
        }

        public async Task<IList<BankAccountModel>> GetAllBankAccountsForUser(int userId)
        {
            using var unitOfWork = _unitOfWork();
            var bankAccounts = (await unitOfWork.BankAccountRepository
                .FindAll(x => x.UserId == userId))
                .OrderBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToList();

            return bankAccounts;
        }

        public async Task<BankAccountModel> GetBankAccountByIdForUser(int userId, int bankAccountId)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingle(x => x.UserId == userId && x.Id == bankAccountId);

            if (bankAccount == null)
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }

            return bankAccount.ToDto();
        }

        public async Task<BankAccountModel> UpdateBankAccount(int userId, BankAccountModel model)
        {
            using var unitOfWork = _unitOfWork();

            var bankAccount = await unitOfWork.BankAccountRepository.FindSingle(x => x.UserId == userId && x.Id == model.Id);

            if (bankAccount != null)
            {
                var recalculateCurrentBalance = bankAccount.StartBalance != model.StartBalance;

                bankAccount.UpdateFromModel(model);
                unitOfWork.BankAccountRepository.Update(bankAccount);

                if (recalculateCurrentBalance)
                {
                    await TransactionHandler.UpdateCurrentBalance(unitOfWork, bankAccount.Id, userId);
                }
                await unitOfWork.SaveAsync();

                return bankAccount.ToDto();
            }
            else
            {
                throw new NotFoundException(nameof(BankAccountEntity));
            }
        }
    }
}
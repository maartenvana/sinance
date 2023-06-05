using Sinance.Business.Exceptions;
using Sinance.Business.Extensions;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinance.Business.Services.BankAccounts;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountCalculationService _bankAccountCalculationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserIdProvider _userIdProvider;

    public BankAccountService(
        IUnitOfWork unitOfWork,
        IBankAccountCalculationService bankAccountCalculationService,
        IUserIdProvider userIdProvider)
    {
        _unitOfWork = unitOfWork;
        _bankAccountCalculationService = bankAccountCalculationService;
        _userIdProvider = userIdProvider;
    }

    public async Task<BankAccountModel> CreateBankAccountForCurrentUser(BankAccountModel model)
    {
        
        var bankAccount = await _unitOfWork.BankAccountRepository.FindSingle(x => x.Name == model.Name);

        if (bankAccount != null)
        {
            throw new AlreadyExistsException(nameof(BankAccountEntity));
        }

        var bankAccountEntity = model.ToNewEntity(_userIdProvider.GetCurrentUserId());
        _unitOfWork.BankAccountRepository.Insert(bankAccountEntity);
        await _unitOfWork.SaveAsync();

        return bankAccountEntity.ToDto();
    }

    public async Task DeleteBankAccountByIdForCurrentUser(int accountId)
    {
        var bankAccount = await _unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == accountId);

        if (bankAccount == null)
        {
            throw new NotFoundException(nameof(BankAccountEntity));
        }

        _unitOfWork.BankAccountRepository.Delete(bankAccount);

        await _unitOfWork.SaveAsync();
    }

    public async Task<List<BankAccountModel>> GetActiveBankAccountsForCurrentUser()
    {
        

        var bankAccounts = (await _unitOfWork.BankAccountRepository
            .FindAll(x => !x.Disabled))
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToList();

        return bankAccounts;
    }

    public async Task<List<BankAccountModel>> GetAllBankAccountsForCurrentUser()
    {
        
        var bankAccounts = (await _unitOfWork.BankAccountRepository
            .ListAll())
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToList();

        return bankAccounts;
    }

    public async Task<BankAccountModel> GetBankAccountByIdForCurrentUser(int bankAccountId)
    {
        

        var bankAccount = await _unitOfWork.BankAccountRepository.FindSingle(x => x.Id == bankAccountId);

        if (bankAccount == null)
        {
            throw new NotFoundException(nameof(BankAccountEntity));
        }

        return bankAccount.ToDto();
    }

    public async Task<BankAccountModel> UpdateBankAccountForCurrentUser(BankAccountModel model)
    {
        

        var bankAccountEntity = await _unitOfWork.BankAccountRepository.FindSingleTracked(x => x.Id == model.Id);

        if (bankAccountEntity == null)
        {
            throw new NotFoundException(nameof(BankAccountEntity));
        }

        var recalculateCurrentBalance = bankAccountEntity.StartBalance != model.StartBalance;

        bankAccountEntity.UpdateFromModel(model);

        if (recalculateCurrentBalance)
        {
            bankAccountEntity.CurrentBalance = await _bankAccountCalculationService.CalculateCurrentBalanceForBankAccount(_unitOfWork, bankAccountEntity);
        }

        _unitOfWork.BankAccountRepository.Update(bankAccountEntity);
        await _unitOfWork.SaveAsync();

        return bankAccountEntity.ToDto();
    }
}
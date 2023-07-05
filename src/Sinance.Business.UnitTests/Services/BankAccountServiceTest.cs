using FluentAssertions;
using Moq;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System.Threading.Tasks;
using Xunit;

namespace Sinance.Business.Tests.Services;

/// <summary>
/// Bank account service tests
/// </summary>
public class BankAccountServiceTest : UnitOfWorkTestBase
{
    private const int _defaultUserId = 1;

    /// <summary>
    /// Constructor/Setup
    /// </summary>
    public BankAccountServiceTest() : base()
    {
        _userIdProvider.CurrentUserId = _defaultUserId;
    }

    /// <summary>
    /// Verify bank account names should be unique for a single user
    /// </summary>
    [Fact]
    public async Task CreateBankAccountForCurrentUser_AccountNameExists_ThrowsException()
    {
        // Arrange
        var bankAccountModel = new BankAccountModel
        {
            AccountType = BankAccountType.Checking,
            Disabled = true,
            Name = "BankAccountName",
            StartBalance = 1000
        };

        InsertEntity(new BankAccountEntity
        {
            Name = bankAccountModel.Name,
            UserId = _defaultUserId
        });

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel));

        // Assert
        await result.Should().ThrowAsync<AlreadyExistsException>();
    }

    [Theory]
    [InlineData(1000, 1000)]
    [InlineData(-1000, -1000)]
    public async Task CreateBankAccountForCurrentUser_NewAccount_CorrectCurrentBalance(decimal startBalance, decimal currentBalance)
    {
        // Arrange
        var bankAccountModel = new BankAccountModel
        {
            AccountType = BankAccountType.Checking,
            Disabled = true,
            Name = "BankAccountName",
            StartBalance = startBalance
        };

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel);

        // Assert
        result.CurrentBalance.Should().Be(currentBalance);
    }

    /// <summary>
    /// Verify new bank accounts are created succesfully
    /// </summary>
    [Fact]
    public async Task CreateBankAccountForCurrentUser_NewAccount_CreatesBankAccount()
    {
        // Arrange
        var bankAccountModel = new BankAccountModel
        {
            AccountType = BankAccountType.Checking,
            Disabled = true,
            Name = "BankAccountName",
            StartBalance = 1000
        };

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel);

        // Assert
        result.Should().NotBeNull();
        result.Should().Match<BankAccountModel>(x =>
            x.AccountType == bankAccountModel.AccountType &&
            x.CurrentBalance == bankAccountModel.StartBalance &&
            x.Disabled == bankAccountModel.Disabled &&
            x.Name == bankAccountModel.Name &&
            x.StartBalance == bankAccountModel.StartBalance);
    }

    [Fact]
    public async Task DeleteBankAccountByIdForCurrentUser_AccountDoesNotExist_ThrowsException()
    {
        // Arrange
        var bankAccountId = 1;

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.DeleteBankAccountByIdForCurrentUser(bankAccountId));

        // Arrange
        await result.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteBankAccountByIdForCurrentUser_AccountExists_DeletesBankAccount()
    {
        // Arrange
        var bankAccountId = 1;

        var bankAccountEntity = new BankAccountEntity
        {
            Id = 1,
            Name = "BankAccountName",
            UserId = _defaultUserId
        };

        InsertEntity(bankAccountEntity);

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        await bankAccountService.DeleteBankAccountByIdForCurrentUser(bankAccountId);

        // Arrange
        var isDeleted = !await EntityExistsById<BankAccountEntity>(bankAccountId);

        Assert.True(isDeleted);
    }

    [Fact]
    public async Task DeleteBankAccountByIdForCurrentUser_UserDoesNotOwnAccount_ThrowsException()
    {
        // Arrange
        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        var bankAccountEntity = new BankAccountEntity
        {
            UserId = _defaultUserId + 1,
            AccountType = BankAccountType.Checking,
            CurrentBalance = 100,
            Name = "BankAccountName",
            StartBalance = 100
        };

        InsertEntity(bankAccountEntity);

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.DeleteBankAccountByIdForCurrentUser(bankAccountEntity.Id));

        // Arrange
        await result.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetActiveBankAccountsForCurrentUser_AccountsExist_ReturnsAllActiveBankAccounts()
    {
        // Arrange
        var checkingAccount = new BankAccountEntity
        {
            Name = "Checking",
            CurrentBalance = 100,
            AccountType = BankAccountType.Checking,
            Disabled = false,
            UserId = _defaultUserId
        };

        var disabledCheckingAccount = new BankAccountEntity
        {
            Name = "DisabledChecking",
            CurrentBalance = 100,
            AccountType = BankAccountType.Checking,
            Disabled = true,
            UserId = _defaultUserId
        };

        var savingsAccount = new BankAccountEntity
        {
            Name = "Savings",
            CurrentBalance = 300,
            StartBalance = 200,
            AccountType = BankAccountType.Savings,
            Disabled = false,
            UserId = _defaultUserId
        };

        InsertEntities(checkingAccount, savingsAccount, disabledCheckingAccount);

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.GetActiveBankAccountsForCurrentUser();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result.Should().Contain(x =>
            x.AccountType == checkingAccount.AccountType &&
            x.CurrentBalance == checkingAccount.CurrentBalance &&
            x.Disabled == checkingAccount.Disabled &&
            x.Id == checkingAccount.Id &&
            x.Name == checkingAccount.Name &&
            x.StartBalance == checkingAccount.StartBalance);

        result.Should().Contain(x =>
            x.AccountType == savingsAccount.AccountType &&
            x.CurrentBalance == savingsAccount.CurrentBalance &&
            x.Disabled == savingsAccount.Disabled &&
            x.Id == savingsAccount.Id &&
            x.Name == savingsAccount.Name &&
            x.StartBalance == savingsAccount.StartBalance);
    }

    [Fact]
    public async Task GetActiveBankAccountsForCurrentUser_NoAccounts_ReturnsEmptyList()
    {
        // Arrange
        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.GetActiveBankAccountsForCurrentUser();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllBankAccountsForCurrentUser_AccountsExist_ReturnsAllBankAccounts()
    {
        // Arrange
        var checkingAccount = new BankAccountEntity
        {
            Name = "Checking",
            CurrentBalance = 100,
            AccountType = BankAccountType.Checking,
            Disabled = false,
            UserId = _defaultUserId
        };

        var disabledCheckingAccount = new BankAccountEntity
        {
            Name = "DisabledChecking",
            CurrentBalance = 100,
            AccountType = BankAccountType.Checking,
            Disabled = true,
            UserId = _defaultUserId
        };

        InsertEntities(checkingAccount, disabledCheckingAccount);

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.GetAllBankAccountsForCurrentUser();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result.Should().Contain(x =>
            x.AccountType == checkingAccount.AccountType &&
            x.CurrentBalance == checkingAccount.CurrentBalance &&
            x.Disabled == checkingAccount.Disabled &&
            x.Id == checkingAccount.Id &&
            x.Name == checkingAccount.Name &&
            x.StartBalance == checkingAccount.StartBalance);

        result.Should().Contain(x =>
            x.AccountType == disabledCheckingAccount.AccountType &&
            x.CurrentBalance == disabledCheckingAccount.CurrentBalance &&
            x.Disabled == disabledCheckingAccount.Disabled &&
            x.Id == disabledCheckingAccount.Id &&
            x.Name == disabledCheckingAccount.Name &&
            x.StartBalance == disabledCheckingAccount.StartBalance);
    }

    [Fact]
    public async Task GetBankAccountByIdForCurrentUser_AccountDoesNotExist_ThrowsException()
    {
        // Arrange
        var bankAccountId = 1;

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.GetBankAccountByIdForCurrentUser(bankAccountId));

        // Arrange
        await result.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBankAccountByIdForCurrentUser_AccountExists_ReturnsBankAccount()
    {
        // Arrange
        var bankAccountId = 1;

        var bankAccountEntity = new BankAccountEntity
        {
            AccountType = BankAccountType.Checking,
            Id = bankAccountId,
            Disabled = true,
            CurrentBalance = 100,
            StartBalance = 100,
            Name = "BankAccount",
            UserId = _defaultUserId
        };

        InsertEntity(bankAccountEntity);

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.GetBankAccountByIdForCurrentUser(bankAccountId);

        // Arrange
        result.Should().NotBeNull();

        result.Should().Match<BankAccountModel>(x =>
            x.AccountType == bankAccountEntity.AccountType &&
            x.CurrentBalance == bankAccountEntity.StartBalance &&
            x.Disabled == bankAccountEntity.Disabled &&
            x.Id == bankAccountEntity.Id &&
            x.Name == bankAccountEntity.Name &&
            x.StartBalance == bankAccountEntity.StartBalance);
    }

    [Fact]
    public async Task GetBankAccountByIdForCurrentUser_UserDoesNotOwnAccount_ThrowsException()
    {
        // Arrange
        var bankAccountEntity = new BankAccountEntity
        {
            AccountType = BankAccountType.Checking,
            Disabled = true,
            CurrentBalance = 100,
            StartBalance = 100,
            Name = "BankAccount",
            UserId = _defaultUserId + 1
        };

        InsertEntity(bankAccountEntity);

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.GetBankAccountByIdForCurrentUser(bankAccountEntity.Id));

        // Arrange
        await result.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateBankAccountForCurrentUser_AccountDoesNotExist_ThrowsException()
    {
        // Arrange
        var bankAccountModel = new BankAccountModel
        {
            Id = 1,
            AccountType = BankAccountType.Checking,
            Disabled = true,
            Name = "BankAccountName",
            StartBalance = 100
        };

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel));

        // Arrange
        await result.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("ChangeAll", BankAccountType.Savings, 200, true)]
    [InlineData("ChangeAccountType", BankAccountType.Savings, 100, false)]
    [InlineData("ChangeStartBalance", BankAccountType.Checking, 1000, false)]
    [InlineData("ChangeDisabled", BankAccountType.Checking, 100, true)]
    [InlineData("ChangeIncludedInProfitLossGraph", BankAccountType.Checking, 100, false)]
    public async Task UpdateBankAccountForCurrentUser_AccountExists_UpdatesBankAccount(
        string name, BankAccountType bankAccountType, decimal startBalance, bool disabled)
    {
        // Arrange
        var bankAccountModel = new BankAccountModel
        {
            Id = 1,
            AccountType = bankAccountType,
            Disabled = disabled,
            Name = name,
            StartBalance = startBalance
        };

        var existingEntity = new BankAccountEntity
        {
            AccountType = BankAccountType.Checking,
            Disabled = false,
            CurrentBalance = 100,
            StartBalance = 100,
            Name = "BankAccountName",
            UserId = _defaultUserId
        };

        InsertEntity(existingEntity);

        if (bankAccountModel.StartBalance != existingEntity.StartBalance)
        {
            _mocker.GetMock<IBankAccountCalculationService>().Setup(x =>
                x.CalculateCurrentBalanceForBankAccount(
                    It.IsAny<IUnitOfWork>(),
                    It.Is<BankAccountEntity>(y => y.Id == existingEntity.Id))).ReturnsAsync(bankAccountModel.StartBalance);
        }

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel);

        // Arrange
        result.Should().NotBeNull();
        result.Should().Match<BankAccountModel>(x =>
            x.AccountType == bankAccountModel.AccountType &&
            x.CurrentBalance == bankAccountModel.StartBalance &&
            x.Disabled == bankAccountModel.Disabled &&
            x.Name == bankAccountModel.Name &&
            x.StartBalance == bankAccountModel.StartBalance);

        _mocker.GetMock<IBankAccountCalculationService>().VerifyAll();
        _mocker.GetMock<IBankAccountCalculationService>().VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBankAccountForCurrentUser_StartBalanceChanged_UpdatesCurrentBalance()
    {
        // Arrange
        var bankAccountModel = new BankAccountModel
        {
            Id = 1,
            AccountType = BankAccountType.Checking,
            Disabled = true,
            Name = "BankAccountName",
            StartBalance = 100
        };

        var bankAccountEntity = new BankAccountEntity
        {
            Name = bankAccountModel.Name,
            UserId = _defaultUserId,
            StartBalance = 200
        };

        InsertEntity(bankAccountEntity);

        _mocker.GetMock<IBankAccountCalculationService>().Setup(x =>
            x.CalculateCurrentBalanceForBankAccount(
                It.IsAny<IUnitOfWork>(),
                It.Is<BankAccountEntity>(x => x.Id == bankAccountEntity.Id))).ReturnsAsync(bankAccountModel.StartBalance).Verifiable();

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel);

        // Assert
        _mocker.GetMock<IBankAccountCalculationService>().VerifyAll();
        _mocker.GetMock<IBankAccountCalculationService>().VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateBankAccountForCurrentUser_UserDoesNotOwnAccount_ThrowsException()
    {
        // Arrange
        var existingEntity = new BankAccountEntity
        {
            AccountType = BankAccountType.Checking,
            Disabled = false,
            CurrentBalance = 100,
            StartBalance = 100,
            Name = "BankAccountName",
            UserId = _defaultUserId + 1
        };

        InsertEntity(existingEntity);

        var bankAccountModel = new BankAccountModel
        {
            Id = existingEntity.Id,
            AccountType = BankAccountType.Checking,
            Name = "OtherName"
        };

        var bankAccountService = _mocker.CreateInstance<BankAccountService>();

        // Act
        var result = FluentActions.Awaiting(async () => await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel));

        // Arrange
        await result.Should().ThrowAsync<NotFoundException>();
    }
}
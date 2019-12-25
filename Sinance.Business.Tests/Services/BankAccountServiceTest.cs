using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Sinance.Business.Exceptions;
using Sinance.Business.Services.Authentication;
using Sinance.Business.Services.BankAccounts;
using Sinance.Communication.Model.BankAccount;
using Sinance.Storage;
using Sinance.Storage.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Sinance.Business.Tests.Services
{
    /// <summary>
    /// Bank account service tests
    /// </summary>
    public class BankAccountServiceTest
    {
        private const int _defaultUserId = 1;
        private readonly Mock<IGenericRepository<BankAccountEntity>> _bankAccountRepositoryMock;
        private readonly AutoMocker _mocker;
        private readonly Mock<IUnitOfWork> _unitOfWork;

        /// <summary>
        /// Constructor/Setup
        /// </summary>
        public BankAccountServiceTest()
        {
            _mocker = new AutoMocker(MockBehavior.Strict);

            _bankAccountRepositoryMock = _mocker.GetMock<IGenericRepository<BankAccountEntity>>();

            _unitOfWork = _mocker.GetMock<IUnitOfWork>();
            _unitOfWork.SetupGet(x => x.BankAccountRepository).Returns(_bankAccountRepositoryMock.Object);
            _unitOfWork.Setup(x => x.Dispose()).Verifiable();

            _mocker.Use<Func<IUnitOfWork>>(() =>
            {
                return _unitOfWork.Object;
            });

            _mocker.Use<IAuthenticationService>(x => x.GetCurrentUserId() == Task.FromResult(_defaultUserId));
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
                IncludeInProfitLossGraph = true,
                Name = "BankAccountName",
                StartBalance = 1000
            };

            SetupFindSingleBankAccount(new BankAccountEntity
            {
                Name = bankAccountModel.Name,
                UserId = _defaultUserId
            });

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = FluentActions.Awaiting(async () => await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel));

            // Assert
            await result.Should().ThrowAsync<AlreadyExistsException>();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
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
                IncludeInProfitLossGraph = true,
                Name = "BankAccountName",
                StartBalance = startBalance
            };

            SetupFindSingleBankAccount(null);

            _bankAccountRepositoryMock.Setup(x => x.Insert(It.IsAny<BankAccountEntity>())).Verifiable();
            _unitOfWork.Setup(x => x.SaveAsync()).ReturnsAsync(0).Verifiable();

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

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
                IncludeInProfitLossGraph = true,
                Name = "BankAccountName",
                StartBalance = 1000
            };

            SetupFindSingleBankAccount(null);

            _bankAccountRepositoryMock
                .Setup(x =>
                    x.Insert(It.Is<BankAccountEntity>(y =>
                        y.AccountType == bankAccountModel.AccountType &&
                        y.CurrentBalance == bankAccountModel.StartBalance &&
                        y.Disabled == bankAccountModel.Disabled &&
                        y.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                        y.Name == bankAccountModel.Name &&
                        y.StartBalance == bankAccountModel.StartBalance
                    ))).Verifiable();

            _unitOfWork.Setup(x => x.SaveAsync()).ReturnsAsync(0).Verifiable();

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel);

            // Assert
            result.Should().NotBeNull();
            result.Should().Match<BankAccountModel>(x =>
                x.AccountType == bankAccountModel.AccountType &&
                x.CurrentBalance == bankAccountModel.StartBalance &&
                x.Disabled == bankAccountModel.Disabled &&
                x.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                x.Name == bankAccountModel.Name &&
                x.StartBalance == bankAccountModel.StartBalance);

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DeleteBankAccountByIdForCurrentUser_AccountDoesNotExist_ThrowsException()
        {
            // Arrange
            var bankAccountId = 1;

            SetupFindSingleBankAccountTracked(null);

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = FluentActions.Awaiting(async () => await bankAccountService.DeleteBankAccountByIdForCurrentUser(bankAccountId));

            // Arrange
            await result.Should().ThrowAsync<NotFoundException>();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DeleteBankAccountByIdForCurrentUser_AccountExists_DeletesBankAccount()
        {// Arrange
            var bankAccountId = 1;

            var bankAccountEntity = new BankAccountEntity
            {
                Id = 1,
                Name = "BankAccountName"
            };

            SetupFindSingleBankAccountTracked(bankAccountEntity);

            _bankAccountRepositoryMock.Setup(x => x.Delete(It.Is<BankAccountEntity>(y => y == bankAccountEntity))).Verifiable();

            _unitOfWork.Setup(x => x.SaveAsync()).ReturnsAsync(0).Verifiable();

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            await bankAccountService.DeleteBankAccountByIdForCurrentUser(bankAccountId);

            // Arrange
            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();

            _unitOfWork.VerifyAll();
            _unitOfWork.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetActiveBankAccountsForCurrentUser_AccountsExist_ReturnsAllBankAccounts()
        {
            // Arrange
            var checkingAccount = new BankAccountEntity
            {
                Id = 1,
                Name = "One",
                CurrentBalance = 100,
                AccountType = BankAccountType.Checking,
                Disabled = false,
                IncludeInProfitLossGraph = false,
            };

            var savingsAccount = new BankAccountEntity
            {
                Id = 2,
                Name = "Two",
                CurrentBalance = 300,
                StartBalance = 200,
                AccountType = BankAccountType.Savings,
                Disabled = false,
                IncludeInProfitLossGraph = true,
            };

            _bankAccountRepositoryMock
                .Setup(x => x.FindAll(It.IsAny<Expression<Func<BankAccountEntity, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(new List<BankAccountEntity>
                {
                    checkingAccount,
                    savingsAccount
                });

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

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
                x.IncludeInProfitLossGraph == checkingAccount.IncludeInProfitLossGraph &&
                x.Name == checkingAccount.Name &&
                x.StartBalance == checkingAccount.StartBalance);

            result.Should().Contain(x =>
                x.AccountType == savingsAccount.AccountType &&
                x.CurrentBalance == savingsAccount.CurrentBalance &&
                x.Disabled == savingsAccount.Disabled &&
                x.Id == savingsAccount.Id &&
                x.IncludeInProfitLossGraph == savingsAccount.IncludeInProfitLossGraph &&
                x.Name == savingsAccount.Name &&
                x.StartBalance == savingsAccount.StartBalance);

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetActiveBankAccountsForCurrentUser_NoAccounts_ReturnsEmptyList()
        {
            // Arrange
            _bankAccountRepositoryMock
                .Setup(x => x.FindAll(It.IsAny<Expression<Func<BankAccountEntity, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(new List<BankAccountEntity>());

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = await bankAccountService.GetActiveBankAccountsForCurrentUser();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetBankAccountByIdForCurrentUser_AccountDoesNotExist_ThrowsException()
        {
            // Arrange
            var bankAccountId = 1;

            SetupFindSingleBankAccount(null);

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = FluentActions.Awaiting(async () => await bankAccountService.GetBankAccountByIdForCurrentUser(bankAccountId));

            // Arrange
            await result.Should().ThrowAsync<NotFoundException>();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
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
                IncludeInProfitLossGraph = true,
                CurrentBalance = 100,
                StartBalance = 100,
                Name = "BankAccount"
            };

            SetupFindSingleBankAccount(bankAccountEntity);

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = await bankAccountService.GetBankAccountByIdForCurrentUser(bankAccountId);

            // Arrange
            result.Should().NotBeNull();

            result.Should().Match<BankAccountModel>(x =>
                x.AccountType == bankAccountEntity.AccountType &&
                x.CurrentBalance == bankAccountEntity.StartBalance &&
                x.Disabled == bankAccountEntity.Disabled &&
                x.Id == bankAccountEntity.Id &&
                x.IncludeInProfitLossGraph == bankAccountEntity.IncludeInProfitLossGraph &&
                x.Name == bankAccountEntity.Name &&
                x.StartBalance == bankAccountEntity.StartBalance);

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
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
                IncludeInProfitLossGraph = true,
                Name = "BankAccountName",
                StartBalance = 100
            };

            SetupFindSingleBankAccountTracked(null);

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = FluentActions.Awaiting(async () => await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel));

            // Arrange
            await result.Should().ThrowAsync<NotFoundException>();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("ChangeAll", BankAccountType.Savings, 200, true, true)]
        [InlineData("ChangeAccountType", BankAccountType.Savings, 100, false, false)]
        [InlineData("ChangeStartBalance", BankAccountType.Checking, 1000, false, false)]
        [InlineData("ChangeDisabled", BankAccountType.Checking, 100, true, false)]
        [InlineData("ChangeIncludedInProfitLossGraph", BankAccountType.Checking, 100, false, true)]
        public async Task UpdateBankAccountForCurrentUser_AccountExists_UpdatesBankAccount(
            string name, BankAccountType bankAccountType, decimal startBalance, bool disabled, bool includedInProfitLossGraph)
        {
            // Arrange
            var bankAccountModel = new BankAccountModel
            {
                Id = 1,
                AccountType = bankAccountType,
                Disabled = disabled,
                IncludeInProfitLossGraph = includedInProfitLossGraph,
                Name = name,
                StartBalance = startBalance
            };

            var existingEntity = new BankAccountEntity
            {
                Id = 1,
                AccountType = BankAccountType.Checking,
                Disabled = false,
                CurrentBalance = 100,
                IncludeInProfitLossGraph = true,
                StartBalance = 100,
                Name = "BankAccountName"
            };

            SetupFindSingleBankAccountTracked(existingEntity);

            if (bankAccountModel.StartBalance != existingEntity.StartBalance)
            {
                _mocker.GetMock<IBankAccountCalculationService>().Setup(x =>
                    x.CalculateCurrentBalanceForBankAccount(
                        It.IsAny<IUnitOfWork>(),
                        It.Is<BankAccountEntity>(y => y == existingEntity))).ReturnsAsync(bankAccountModel.StartBalance);
            }

            _bankAccountRepositoryMock.Setup(x =>
                x.Update(It.Is<BankAccountEntity>(y =>
                    y.AccountType == bankAccountModel.AccountType &&
                    y.CurrentBalance == bankAccountModel.StartBalance &&
                    y.Disabled == bankAccountModel.Disabled &&
                    y.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                    y.Name == bankAccountModel.Name &&
                    y.StartBalance == bankAccountModel.StartBalance))).Verifiable();

            _unitOfWork.Setup(x => x.SaveAsync()).ReturnsAsync(0).Verifiable();

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            var result = await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel);

            // Arrange
            result.Should().NotBeNull();
            result.Should().Match<BankAccountModel>(x =>
                x.AccountType == bankAccountModel.AccountType &&
                x.CurrentBalance == bankAccountModel.StartBalance &&
                x.Disabled == bankAccountModel.Disabled &&
                x.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                x.Name == bankAccountModel.Name &&
                x.StartBalance == bankAccountModel.StartBalance);

            _mocker.GetMock<IBankAccountCalculationService>().VerifyAll();
            _mocker.GetMock<IBankAccountCalculationService>().VerifyNoOtherCalls();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
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
                IncludeInProfitLossGraph = true,
                Name = "BankAccountName",
                StartBalance = 100
            };

            var bankAccountEntity = new BankAccountEntity
            {
                Id = 1,
                Name = bankAccountModel.Name,
                UserId = _defaultUserId,
                StartBalance = 200
            };

            _unitOfWork.Setup(x => x.SaveAsync()).ReturnsAsync(0).Verifiable();

            SetupFindSingleBankAccountTracked(bankAccountEntity);

            _mocker.GetMock<IBankAccountCalculationService>().Setup(x =>
                x.CalculateCurrentBalanceForBankAccount(
                    It.IsAny<IUnitOfWork>(),
                    It.Is<BankAccountEntity>(x => x == bankAccountEntity))).ReturnsAsync(bankAccountModel.StartBalance).Verifiable();

            _bankAccountRepositoryMock
                            .Setup(x => x.Update(It.Is<BankAccountEntity>(y => y == bankAccountEntity)))
                            .Verifiable();

            var bankAccountService = _mocker.CreateInstance<BankAccountService>(true);

            // Act
            await bankAccountService.UpdateBankAccountForCurrentUser(bankAccountModel);

            // Assert
            _mocker.GetMock<IBankAccountCalculationService>().VerifyAll();
            _mocker.GetMock<IBankAccountCalculationService>().VerifyNoOtherCalls();

            _bankAccountRepositoryMock.VerifyAll();
            _bankAccountRepositoryMock.VerifyNoOtherCalls();
        }

        private void SetupFindSingleBankAccount(BankAccountEntity bankAccountEntity)
        {
            _bankAccountRepositoryMock
                            .Setup(x => x.FindSingle(It.IsAny<Expression<Func<BankAccountEntity, bool>>>(), It.IsAny<string[]>()))
                            .ReturnsAsync(bankAccountEntity);
        }

        private void SetupFindSingleBankAccountTracked(BankAccountEntity bankAccountEntity)
        {
            _bankAccountRepositoryMock
                            .Setup(x => x.FindSingleTracked(It.IsAny<Expression<Func<BankAccountEntity, bool>>>(), It.IsAny<string[]>()))
                            .ReturnsAsync(bankAccountEntity);
        }
    }
}
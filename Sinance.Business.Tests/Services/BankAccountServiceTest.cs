using FluentAssertions;
using Moq;
using Moq.AutoMock;
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
    public class BankAccountServiceTest
    {
        private const int _defaultUserId = 1;
        private readonly Mock<IGenericRepository<BankAccountEntity>> _bankAccountRepositoryMock;
        private readonly AutoMocker _mocker;

        public BankAccountServiceTest()
        {
            _mocker = new AutoMocker();

            _bankAccountRepositoryMock = _mocker.GetMock<IGenericRepository<BankAccountEntity>>();

            _mocker.Use<Func<IUnitOfWork>>(() =>
            {
                var unitOfWork = _mocker.GetMock<IUnitOfWork>();
                unitOfWork.SetupGet(x => x.BankAccountRepository).Returns(_bankAccountRepositoryMock.Object);

                return unitOfWork.Object;
            });

            _mocker.Use<IAuthenticationService>(x => x.GetCurrentUserId() == Task.FromResult(_defaultUserId));
        }

        [Fact]
        public async Task CreateBankAccountForCurrentUser_CreatesBankAccount()
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

            _mocker.GetMock<IGenericRepository<BankAccountEntity>>()
                .Setup(x => x.FindAll(It.IsAny<Expression<Func<BankAccountEntity, bool>>>(), It.IsAny<string[]>()))
                .ReturnsAsync(new List<BankAccountEntity>());

            // Act
            var bankAccountService = _mocker.CreateInstance<BankAccountService>();
            var createResult = await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel);

            // Assert
            createResult.Should().NotBeNull();
            createResult.Should().Match<BankAccountModel>(x =>
                x.AccountType == bankAccountModel.AccountType &&
                x.CurrentBalance == bankAccountModel.StartBalance &&
                x.Disabled == bankAccountModel.Disabled &&
                x.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                x.Name == bankAccountModel.Name &&
                x.StartBalance == bankAccountModel.StartBalance);

            _mocker.Verify<IGenericRepository<BankAccountEntity>>(x =>
                x.Insert(It.Is<BankAccountEntity>(y =>
                    y.AccountType == bankAccountModel.AccountType &&
                    y.CurrentBalance == bankAccountModel.StartBalance &&
                    y.Disabled == bankAccountModel.Disabled &&
                    y.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                    y.Name == bankAccountModel.Name &&
                    y.StartBalance == bankAccountModel.StartBalance
            )));
        }
    }
}
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
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Sinance.Business.Tests.Services
{
    public class BankAccountServiceTest
    {
        private const int _defaultUserId = 1;
        private readonly AutoMocker _mocker;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public BankAccountServiceTest()
        {
            _mocker = new AutoMocker();
            _unitOfWorkMock = _mocker.GetMock<IUnitOfWork>();

            _mocker.Use<IAuthenticationService>(x => x.GetCurrentUserId() == Task.FromResult(_defaultUserId));
            _mocker.GetMock<Func<IUnitOfWork>>().SetReturnsDefault(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateBankAccountForCurrentUser_CreatesBankAccount()
        {
            // Arrange
            var bankAccountModel = new BankAccountModel
            {
                AccountType = BankAccountType.Checking,
                CurrentBalance = 500,
                Disabled = true,
                IncludeInProfitLossGraph = true,
                Name = "BankAccountName",
                StartBalance = 1000
            };

            _mocker.Use<IGenericRepository<BankAccountEntity>>(x => x.FindAll(It.IsAny<Expression<Func<BankAccountEntity, bool>>>(), It.IsAny<string[]>()) == null);

            // Act
            var bankAccountService = _mocker.CreateInstance<BankAccountService>();
            var createResult = await bankAccountService.CreateBankAccountForCurrentUser(bankAccountModel);

            // Assert
            createResult.Should().NotBeNull();
            createResult.Should().Match<BankAccountModel>(x =>
                x.AccountType == bankAccountModel.AccountType &&
                x.CurrentBalance == bankAccountModel.CurrentBalance &&
                x.Disabled == bankAccountModel.Disabled &&
                x.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                x.Name == bankAccountModel.Name &&
                x.StartBalance == bankAccountModel.StartBalance);

            _mocker.Verify<IGenericRepository<BankAccountEntity>>(x =>
                x.Insert(It.Is<BankAccountEntity>(y =>
                    y.AccountType == bankAccountModel.AccountType &&
                    y.CurrentBalance == bankAccountModel.CurrentBalance &&
                    y.Disabled == bankAccountModel.Disabled &&
                    y.IncludeInProfitLossGraph == bankAccountModel.IncludeInProfitLossGraph &&
                    y.Name == bankAccountModel.Name &&
                    y.StartBalance == bankAccountModel.StartBalance
            )));
        }
    }
}
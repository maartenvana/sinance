using System;
using Finances.Domain.Entities;
using Finances.Models;
using Finances.UnitTestBase.Classes;

namespace Finances.Web.Tests.Controllers
{
    /// <summary>
    /// Contains all test data setup for the bank account controller tests
    /// </summary>
    public class BankAccountControllerTestData : FinanceTestData
    {
        /// <summary>
        /// Creates a bank account model based on a bank account entity
        /// </summary>
        /// <returns>Created bank account model</returns>
        public static BankAccountModel CreateBankAccountModel(BankAccount bankAccount)
        {
            if (bankAccount == null)
                throw new ArgumentNullException(nameof(bankAccount));

            return new BankAccountModel
            {
                Id = bankAccount.Id,
                Name = bankAccount.Name,
                CurrentBalance = bankAccount.CurrentBalance,
                StartBalance = bankAccount.StartBalance
            };
        }
    }
}

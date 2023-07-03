using MediatR;
using Sinance.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinance.Domain.Events
{
    public class AccountTransactionAmountChangedDomainEvent : INotification
    {
        public AccountTransactionAmountChangedDomainEvent(AccountTransaction transaction, decimal oldAmount)
        {
            Transaction = transaction;
            OldAmount = oldAmount;
        }

        public decimal OldAmount { get; }
        public AccountTransaction Transaction { get; }
    }
}

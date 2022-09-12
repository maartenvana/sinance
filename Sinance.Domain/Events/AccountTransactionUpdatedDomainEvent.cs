using MediatR;
using Sinance.Domain.Model;

namespace Sinance.Domain.Events
{
    public class AccountTransactionUpdatedDomainEvent : INotification
    {
        public AccountTransaction Transaction { get; }

        public AccountTransactionUpdatedDomainEvent(AccountTransaction transaction)
        {
            Transaction = transaction; 
        }
    }
}

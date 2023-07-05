using MediatR;
using Sinance.Domain.Model;

namespace Sinance.Domain.Events;

public class AccountTransactionCreatedDomainEvent : INotification
{
    public AccountTransaction Transaction { get; }

    public AccountTransactionCreatedDomainEvent(AccountTransaction transaction)
    {
        Transaction = transaction; 
    }
}

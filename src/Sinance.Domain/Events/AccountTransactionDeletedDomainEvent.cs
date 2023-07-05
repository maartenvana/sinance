using MediatR;
using Sinance.Domain.Model;

namespace Sinance.Domain.Events;

public class AccountTransactionDeletedDomainEvent : INotification
{
    public AccountTransaction Transaction { get; }

    public AccountTransactionDeletedDomainEvent(AccountTransaction transaction)
    {
        Transaction = transaction;
    }
}
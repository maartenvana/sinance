using Sinance.Domain.Events;
using Sinance.Infrastructure;

namespace Sinance.Application.DomainEventHandlers
{
    public class BalanceRecalculationEventHandler :
        INotificationHandler<AccountTransactionAmountChangedDomainEvent>,
        INotificationHandler<AccountTransactionCreatedDomainEvent>,
        INotificationHandler<AccountTransactionDeletedDomainEvent>
    {
        private readonly SinanceContext context;

        public BalanceRecalculationEventHandler(SinanceContext context)
        {
            this.context = context;
        }

        public async Task Handle(AccountTransactionDeletedDomainEvent notification, CancellationToken cancellationToken)
        {
            await RecalculateBalanceForAccount(notification.Transaction.BankAccountId);
        }

        public async Task Handle(AccountTransactionCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await RecalculateBalanceForAccount(notification.Transaction.BankAccountId);
        }

        public async Task Handle(AccountTransactionAmountChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            await RecalculateBalanceForAccount(notification.Transaction.BankAccountId);
        }

        private Task RecalculateBalanceForAccount(int bankAccountId)
        {
            var bankAccount = context.BankAccounts.Single(x => x.Id == bankAccountId);
            var bankAccountTransactionsSum = context.Transactions.Where(x => x.BankAccountId == bankAccountId).Sum(x => x.Amount);

            bankAccount.CurrentBalance = bankAccount.StartBalance + bankAccountTransactionsSum;

            return Task.CompletedTask;
        }
    }
}

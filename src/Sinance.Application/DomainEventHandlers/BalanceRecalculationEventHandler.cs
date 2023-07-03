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
            await UpdateBankAccountCurrentBalance(notification.Transaction.BankAccountId, changeAmount: notification.Transaction.Amount * -1);
        }

        public async Task Handle(AccountTransactionCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await UpdateBankAccountCurrentBalance(notification.Transaction.BankAccountId, changeAmount: notification.Transaction.Amount);
        }

        public async Task Handle(AccountTransactionAmountChangedDomainEvent notification, CancellationToken cancellationToken)
        {
            var changeAmount = Math.Abs(notification.OldAmount - notification.Transaction.Amount);
            await UpdateBankAccountCurrentBalance(notification.Transaction.BankAccountId, changeAmount);
        }

        private Task UpdateBankAccountCurrentBalance(int bankAccountId, decimal changeAmount)
        {
            var bankAccount = context.BankAccounts.Single(x => x.Id == bankAccountId);

            bankAccount.CurrentBalance += changeAmount;

            return Task.CompletedTask;
        }
    }
}

using Sinance.Infrastructure;

namespace Sinance.Application.Command.Transaction
{
    public class DeleteAccountTransactionCommandHandler : IRequestHandler<DeleteAccountTransactionCommand>
    {
        private readonly SinanceContext context;

        public DeleteAccountTransactionCommandHandler(SinanceContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle(DeleteAccountTransactionCommand request, CancellationToken cancellationToken)
        {
            var transaction = context.Transactions.SingleOrDefault(x => x.Id == request.TransactionId);

            context.Transactions.Remove(transaction);

            await context.SaveChangesAsync();

            return Unit.Value;
        }
    }
}

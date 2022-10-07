using Sinance.Application.Model;
using Sinance.Domain.Model;
using Sinance.Infrastructure;

namespace Sinance.Application.Command.Transaction
{
    public class UpdateAccountTransactionCommandHandler : IRequestHandler<UpdateAccountTransactionCommand, AccountTransaction>
    {
        private readonly SinanceContext context;

        public UpdateAccountTransactionCommandHandler(SinanceContext context)
        {
            this.context = context;
        }

        public async Task<AccountTransaction> Handle(UpdateAccountTransactionCommand request, CancellationToken cancellationToken)
        {
            var currentTransaction = context.Transactions.SingleOrDefault(x => x.Id == request.TransactionId);

            UpdateTransactionFromUpdateModel(currentTransaction, request.UpdateModel);

            await context.SaveChangesAsync(cancellationToken);

            return currentTransaction;
        }

        private static void UpdateTransactionFromUpdateModel(AccountTransaction currentTransaction, AccountTransactionUpdateModel updateModel)
        {
            var oldAmount = currentTransaction.Amount;

            currentTransaction.UpdateMetadata(updateModel.Date, updateModel.Name, updateModel.Description, updateModel.CategoryId, updateModel.SourceAccountNumber, updateModel.DestinationAccountNumber);

            if (currentTransaction.Amount != updateModel.Amount)
                currentTransaction.UpdateAmount(updateModel.Amount, oldAmount);
        }
    }
}
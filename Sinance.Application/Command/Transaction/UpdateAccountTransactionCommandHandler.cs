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
            currentTransaction.Description = updateModel.Description;
            currentTransaction.Name = updateModel.Name;
            currentTransaction.Amount = updateModel.Amount;
            currentTransaction.Date = updateModel.Date;
            currentTransaction.AccountNumber = updateModel.SourceAccountNumber;
            currentTransaction.DestinationAccount = updateModel.DestinationAccountNumber;
            currentTransaction.CategoryId = updateModel.CategoryId;
        }
    }
}